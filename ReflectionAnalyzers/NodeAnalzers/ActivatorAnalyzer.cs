namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ActivatorAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL026MissingDefaultConstructor.Descriptor);

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
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                invocation.TryGetTarget(KnownSymbol.Activator.CreateInstance, context.SemanticModel, context.CancellationToken, out var createInstance))
            {
                if (createInstance.IsGenericMethod &&
                    memberAccess.Name is GenericNameSyntax genericName &&
                    genericName.TypeArgumentList is TypeArgumentListSyntax typeArgumentList &&
                    typeArgumentList.Arguments.TrySingle(out var typeArgument) &&
                    context.SemanticModel.TryGetType(typeArgument, context.CancellationToken, out var type) &&
                    type is INamedTypeSymbol namedType &&
                    (!namedType.Constructors.TrySingle(x => x.Parameters.Length == 0, out var ctor) ||
                     ctor.DeclaredAccessibility != Accessibility.Public))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL026MissingDefaultConstructor.Descriptor, typeArgument.GetLocation(), type.ToDisplayString()));
                }
            }
        }
    }
}
