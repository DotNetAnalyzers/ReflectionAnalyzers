namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetAccessorAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL003MemberDoesNotExist.Descriptor);

        /// <inheritdoc/>
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
                    PropertyInfo.TryGet(propertyInfoAccess.Expression, context, out var propertyInfo) &&
                    propertyInfo.Property.GetMethod == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, invocation.GetNameLocation()));
                }
                else if (invocation.TryGetTarget(KnownSymbol.PropertyInfo.GetSetMethod, context.SemanticModel, context.CancellationToken, out _) &&
                         PropertyInfo.TryGet(propertyInfoAccess.Expression, context, out propertyInfo) &&
                         propertyInfo.Property.SetMethod == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, invocation.GetNameLocation()));
                }
            }
        }

        private static void HandleMemberAccess(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax invocation } memberAccess)
            {
                if (IsProperty(memberAccess, KnownSymbol.PropertyInfo.GetMethod, context) &&
                    PropertyInfo.TryGet(invocation, context, out var propertyInfo) &&
                    propertyInfo.Property.GetMethod == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, memberAccess.Name.GetLocation()));
                }
                else if (IsProperty(memberAccess, KnownSymbol.PropertyInfo.SetMethod, context) &&
                         PropertyInfo.TryGet(invocation, context, out propertyInfo) &&
                         propertyInfo.Property.SetMethod == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, memberAccess.Name.GetLocation()));
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
}
