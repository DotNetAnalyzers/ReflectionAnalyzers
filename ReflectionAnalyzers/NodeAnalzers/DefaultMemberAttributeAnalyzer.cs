namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Attribute = Gu.Roslyn.AnalyzerExtensions.Attribute;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DefaultMemberAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL046DefaultMemberMustExist.Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (!(context.Node is AttributeSyntax attribute))
            {
                return;
            }

            if (!Attribute.IsType(attribute, KnownSymbol.DefaultMemberAttribute, context.SemanticModel, context.CancellationToken))
            {
                return;
            }

            if (!Attribute.TryFindArgument(attribute, 0, "memberName", out var argument))
            {
                return;
            }

            if (!context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string memberName))
            {
                return;
            }

            if (!attribute.TryFirstAncestor(out ClassDeclarationSyntax classDeclaration)) { return; }

            // try to find the member
            if (classDeclaration.TryFindMethod(memberName, out var memberNode)) { return; }
            if (classDeclaration.TryFindProperty(memberName, out var propertyNode)) { return; }
            if (classDeclaration.TryFindField(memberName, out var fieldNode)) { return; }
            if (classDeclaration.TryFindEvent(memberName, out var eventNode)) { return; }

            context.ReportDiagnostic(Diagnostic.Create(REFL046DefaultMemberMustExist.Descriptor, argument.GetLocation(), "not found"));
        }
    }
}
