namespace ReflectionAnalyzers;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class IsDefinedAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.REFL044ExpectedAttributeType);

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
            if (!attributeType.Value.IsAssignableTo(KnownSymbol.Attribute, context.Compilation))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.REFL044ExpectedAttributeType,
                        attributeType.Argument.GetLocation()));
            }
        }
    }

    private static bool TryGetArgs(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out IMethodSymbol? target, [NotNullWhen(true)] out ExpressionSyntax? member, out ArgumentAndValue<ITypeSymbol> attributeType, out ArgumentSyntax? inheritsArg)
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
        attributeType = default;
        inheritsArg = null;
        return false;
    }
}
