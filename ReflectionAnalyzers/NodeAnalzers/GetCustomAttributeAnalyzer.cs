namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetCustomAttributeAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL010PreferGenericGetCustomAttribute.Descriptor);

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
                invocation.TryGetTarget(KnownSymbol.Attribute.GetCustomAttribute, context.SemanticModel, context.CancellationToken, out var target) &&
                target.Parameters.Length == 2 &&
                target.Parameters[1].Type == KnownSymbol.Type &&
                invocation.TryFindArgument(target.Parameters[0], out var memberArg) &&
                memberArg.Expression is ExpressionSyntax member &&
                invocation.TryFindArgument(target.Parameters[1], out var typeArg) &&
                typeArg.Expression is TypeOfExpressionSyntax typeOf &&
                typeOf.Type is TypeSyntax type &&
                invocation.Parent?.IsEither(SyntaxKind.CastExpression, SyntaxKind.AsExpression) == true)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        REFL010PreferGenericGetCustomAttribute.Descriptor,
                        invocation.GetLocation(),
                        ImmutableDictionary.CreateRange(new[]
                        {
                            new KeyValuePair<string, string>(nameof(ExpressionSyntax), member.ToString()),
                            new KeyValuePair<string, string>(nameof(TypeSyntax), type.ToString()),
                        })));
            }
        }
    }
}
