namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ArgumentAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL027PreferTypeEmptyTypes.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.Argument);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is ArgumentSyntax argument &&
                ShouldCheck(argument) &&
                Array.IsCreatingEmpty(argument.Expression, context) &&
                context.SemanticModel.TryGetType(argument.Expression, context.CancellationToken, out var type) &&
                type is IArrayTypeSymbol arrayType &&
                arrayType.ElementType == KnownSymbol.Type)
            {
                context.ReportDiagnostic(Diagnostic.Create(REFL027PreferTypeEmptyTypes.Descriptor, argument.GetLocation()));
            }
        }

        private static bool ShouldCheck(ArgumentSyntax argument)
        {
            switch (argument.Expression.Kind())
            {
                case SyntaxKind.InvocationExpression:
                case SyntaxKind.ArrayCreationExpression:
                case SyntaxKind.ImplicitArrayCreationExpression:
                    return true;
                default:
                    return false;
            }
        }
    }
}
