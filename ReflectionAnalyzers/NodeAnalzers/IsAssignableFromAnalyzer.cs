namespace ReflectionAnalyzers;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class IsAssignableFromAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.REFL040PreferIsInstanceOfType);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
    }

    internal static bool IsInstanceGetType(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ExpressionSyntax? instance)
    {
        if (expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } temp, Name.Identifier.ValueText: "GetType" }, ArgumentList.Arguments.Count: 0 } invocation &&
            invocation.TryGetTarget(KnownSymbol.Object.GetType, semanticModel, cancellationToken, out _))
        {
            instance = temp;
            return true;
        }

        instance = null;
        return false;
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.Node is InvocationExpressionSyntax invocation &&
            invocation.TryGetTarget(KnownSymbol.Type.IsAssignableFrom, QualifiedParameter.Create(KnownSymbol.Type), context.SemanticModel, context.CancellationToken, out _, out var arg) &&
            Type.TryGet(arg.Expression, context.SemanticModel, context.CancellationToken, out _, out var source) &&
            IsInstanceGetType(source, context.SemanticModel, context.CancellationToken, out var instance))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.REFL040PreferIsInstanceOfType,
                    invocation.GetLocation(),
                    invocation.Expression,
                    instance));
        }
    }
}
