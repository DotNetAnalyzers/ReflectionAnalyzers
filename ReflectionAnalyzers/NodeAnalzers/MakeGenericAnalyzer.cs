namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class MakeGenericAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.REFL031UseCorrectGenericArguments,
            Descriptors.REFL034DoNotMakeGeneric);

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
                TypeArguments.Find(invocation, context.SemanticModel, context.CancellationToken) is { } typeArguments)
            {
                if (typeArguments.Symbol.IsGenericDefinition() &&
                    typeArguments.Parameters.Length != typeArguments.Arguments.Length)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.REFL031UseCorrectGenericArguments,
                            invocation.ArgumentList.GetLocation(),
                            $"The number of generic arguments provided doesn't equal the arity of the generic type definition. The member has {typeArguments.Parameters.Length} parameter{PluralS(typeArguments.Parameters.Length)} but {typeArguments.Arguments.Length} argument{PluralS(typeArguments.Arguments.Length)} are passed in."));
                }
                else if (typeArguments.TryFindConstraintViolation(context.SemanticModel, context.CancellationToken, out var argument, out var parameter))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.REFL031UseCorrectGenericArguments,
                            argument.GetLocation(),
                            $"The argument {argument}, on '{typeArguments.Symbol}' violates the constraint of type '{parameter}'."));
                }
                else if (!typeArguments.Symbol.IsGenericDefinition())
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.REFL034DoNotMakeGeneric,
                            invocation.GetNameLocation(),
#pragma warning disable SA1118 // Parameter should not span multiple lines
                            typeArguments.Symbol is ITypeSymbol
                                ? $"{typeArguments.Symbol} is not a GenericTypeDefinition. MakeGenericType may only be called on a type for which Type.IsGenericTypeDefinition is true."
                                : $"{typeArguments.Symbol} is not a GenericMethodDefinition. MakeGenericMethod may only be called on a method for which MethodBase.IsGenericMethodDefinition is true."));
#pragma warning restore SA1118 // Parameter should not span multiple lines
                }
            }

            static string PluralS(int i) => i == 1 ? string.Empty : "s";
        }
    }
}
