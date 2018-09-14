namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetMethodAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL003GetMethodTargetDoesNotExist.Descriptor);

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
                invocation.TryGetMethodName(out var targetName) &&
                targetName == "GetMethod" &&
                invocation.ArgumentList is ArgumentListSyntax argumentList &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is TypeOfExpressionSyntax typeOf &&
                context.SemanticModel.TryGetSymbol(invocation, context.CancellationToken, out var getMethod) &&
                getMethod == KnownSymbol.Type.GetMethod &&
                context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var type))
            {
                if (argumentList.Arguments.TryFirst(out var nameArg) &&
                    nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var reflectionTargetName))
                {
                    if (type.TryFindFirstMethodRecursive(reflectionTargetName, out var reflectionTarget))
                    {

                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL003GetMethodTargetDoesNotExist.Descriptor, nameArg.GetLocation(), type, reflectionTargetName));
                    }
                }
            }
        }
    }
}
