namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal class IsDefinedAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL044ExpectedAttributeType.Descriptor);

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
                TryGetArgs(invocation, context, out _, out _, out var attributeType, out _))
            {
                if (!attributeType.Value.IsAssignableTo(context.Compilation.GetTypeByMetadataName("System.Attribute"), context.Compilation))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL044ExpectedAttributeType.Descriptor,
                            attributeType.Argument.GetLocation()));
                }
            }
        }

        private static bool TryGetArgs(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out IMethodSymbol target, out ExpressionSyntax member, out ArgumentAndValue<ITypeSymbol> attributeType, out ArgumentSyntax inheritsArg)
        {
            if ((invocation.TryGetTarget(KnownSymbol.Attribute.IsDefined,                 context.SemanticModel, context.CancellationToken, out target) ||
                 invocation.TryGetTarget(KnownSymbol.CustomAttributeExtensions.IsDefined, context.SemanticModel, context.CancellationToken, out target)) &&
                target.TryFindParameter("attributeType", out var attributeTypeParameter) &&
                invocation.TryFindArgument(attributeTypeParameter, out var attributeTypeArg) &&
                attributeTypeArg.Expression is TypeOfExpressionSyntax typeOf &&
                context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var typeSymbol))
            {
                attributeType = new ArgumentAndValue<ITypeSymbol>(attributeTypeArg, typeSymbol);
                if (!target.TryFindParameter("inherit", out var inheritParameter) ||
                    !invocation.TryFindArgument(inheritParameter, out inheritsArg))
                {
                    inheritsArg = null;
                }

                if (target.IsExtensionMethod &&
                    invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    member = memberAccess.Expression;
                    return true;
                }

                if (target.TryFindParameter("element", out var elementParameter) &&
                    invocation.TryFindArgument(elementParameter, out var elementArg))
                {
                    member = elementArg.Expression;
                    return true;
                }
            }

            member = null;
            attributeType = default(ArgumentAndValue<ITypeSymbol>);
            inheritsArg = null;
            return false;
        }
    }
}
