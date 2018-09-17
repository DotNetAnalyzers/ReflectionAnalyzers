namespace ReflectionAnalyzers
{
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
            REFL010PreferGenericGetCustomAttribute.Descriptor,
            REFL012PreferIsDefined.Descriptor);

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
                typeOf.Type is TypeSyntax attributeTYpe)
            {
                if (invocation.Parent?.IsEither(SyntaxKind.CastExpression, SyntaxKind.AsExpression) == true)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL010PreferGenericGetCustomAttribute.Descriptor,
                            invocation.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(nameof(InvocationExpressionSyntax), $"{member}.GetCustomAttribute<{attributeTYpe}>()"),
                            attributeTYpe.ToString()));
                }

                if (invocation.Parent is BinaryExpressionSyntax binary &&
                    binary.Right.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    if (binary.IsKind(SyntaxKind.EqualsExpression))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL012PreferIsDefined.Descriptor,
                                binary.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(nameof(InvocationExpressionSyntax), $"!Attribute.IsDefined({member}, typeof({attributeTYpe}))")));
                    }
                    else if (binary.IsKind(SyntaxKind.NotEqualsExpression))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL012PreferIsDefined.Descriptor,
                                binary.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(nameof(InvocationExpressionSyntax), $"Attribute.IsDefined({member}, typeof({attributeTYpe}))")));
                    }
                }
            }
        }
    }
}
