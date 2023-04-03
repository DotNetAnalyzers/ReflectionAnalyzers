namespace ReflectionAnalyzers;

using System.Collections.Immutable;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class GetAccessorAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.REFL003MemberDoesNotExist);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(c => HandleInvocation(c), SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(c => HandleMemberAccess(c), SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void HandleInvocation(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.Node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax propertyInfoAccess } invocation)
        {
            if (invocation.TryGetTarget(KnownSymbol.PropertyInfo.GetGetMethod, context.SemanticModel, context.CancellationToken, out _) &&
                PropertyInfo.Find(propertyInfoAccess.Expression, context.SemanticModel, context.CancellationToken) is { Property.GetMethod: null })
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL003MemberDoesNotExist, invocation.GetNameLocation()));
            }
            else if (invocation.TryGetTarget(KnownSymbol.PropertyInfo.GetSetMethod, context.SemanticModel, context.CancellationToken, out _) &&
                     PropertyInfo.Find(propertyInfoAccess.Expression, context.SemanticModel, context.CancellationToken) is { Property.SetMethod: null })
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL003MemberDoesNotExist, invocation.GetNameLocation()));
            }
        }
    }

    private static void HandleMemberAccess(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.Node is MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation } memberAccess)
        {
            if (IsProperty(memberAccess, KnownSymbol.PropertyInfo.GetMethod, context) &&
                PropertyInfo.Find(invocation, context.SemanticModel, context.CancellationToken) is { Property.GetMethod: null })
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL003MemberDoesNotExist, memberAccess.Name.GetLocation()));
            }
            else if (IsProperty(memberAccess, KnownSymbol.PropertyInfo.SetMethod, context) &&
                     PropertyInfo.Find(invocation, context.SemanticModel, context.CancellationToken) is { Property.SetMethod: null })
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL003MemberDoesNotExist, memberAccess.Name.GetLocation()));
            }
        }
    }

    private static bool IsProperty(MemberAccessExpressionSyntax memberAccess, QualifiedProperty property, SyntaxNodeAnalysisContext context)
    {
        return memberAccess.Name is { } name &&
               name.Identifier.ValueText == property.Name &&
               context.SemanticModel.TryGetSymbol(memberAccess, context.CancellationToken, out IPropertySymbol? propertySymbol) &&
               propertySymbol == property;
    }
}
