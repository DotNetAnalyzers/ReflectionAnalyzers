namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetInterfaceAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL020AmbiguousMatchInterface.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                invocation.TryGetTarget(KnownSymbol.Type.GetInterface, context.SemanticModel, context.CancellationToken, out var getInterface) &&
                getInterface.TryFindParameter(KnownSymbol.String, out var nameParameter) &&
                invocation.TryFindArgument(nameParameter, out var nameArg) &&
                TryGetName(nameArg, context, out var name) &&
                GetX.TryGetType(invocation, context, out var type, out _))
            {
                var count = type.AllInterfaces.Count(x => IsMatch(x));
                if (count > 1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL020AmbiguousMatchInterface.Descriptor, nameArg.GetLocation()));
                }
            }

            bool IsMatch(ITypeSymbol candidate)
            {
                if (candidate.MetadataName == name)
                {
                    return true;
                }

                return name.IsParts(candidate.ContainingNamespace.ToString(), ".", candidate.MetadataName);
            }
        }

        private static bool TryGetName(ArgumentSyntax nameArg, SyntaxNodeAnalysisContext context, out string name)
        {
            switch (nameArg.Expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    if (memberAccess.Expression is TypeOfExpressionSyntax typeOf &&
                        context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var type))
                    {
                        if (memberAccess.Name.Identifier.ValueText == "Name")
                        {
                            name = type.MetadataName;
                            return true;
                        }

                        if (memberAccess.Name.Identifier.ValueText == "FullName")
                        {
                            name = $"{type.ContainingNamespace}.{type.MetadataName}";
                            return true;
                        }
                    }

                    name = null;
                    return false;
                default:
                    return context.SemanticModel.TryGetConstantValue(nameArg.Expression, context.CancellationToken, out name);
            }
        }
    }
}
