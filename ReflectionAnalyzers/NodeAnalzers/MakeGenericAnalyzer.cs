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
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL031UseCorrectGenericArguments.Descriptor);

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
                (TypeArguments.TryCreate(invocation, KnownSymbol.MethodInfo.MakeGenericType, context, out var typeArguments) ||
                 TypeArguments.TryCreate(invocation, KnownSymbol.MethodInfo.MakeGenericMethod, context, out typeArguments)))
            {
                if (typeArguments.GenericDefinition.TypeParameters.Length != typeArguments.Types.Length)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor, invocation.ArgumentList.GetLocation()));
                }
                else if (!typeArguments.TryFindMisMatch(context.Compilation, out var mismatch))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor, mismatch.GetLocation()));
                }
            }
        }

        private struct TypeArguments
        {
            internal readonly ArgumentSyntax Argument;
            internal readonly INamedTypeSymbol GenericDefinition;
            internal readonly ImmutableArray<ITypeSymbol> Types;

            public TypeArguments(ArgumentSyntax argument, INamedTypeSymbol genericDefinition, ImmutableArray<ITypeSymbol> types)
            {
                this.Argument = argument;
                this.GenericDefinition = genericDefinition;
                this.Types = types;
            }

            internal static bool TryCreate(InvocationExpressionSyntax invocation, QualifiedMethod expected, SyntaxNodeAnalysisContext context, out TypeArguments typeArguments)
            {
                if (invocation?.ArgumentList is ArgumentListSyntax argumentList &&
                    invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    invocation.TryGetTarget(KnownSymbol.MethodInfo.MakeGenericType, context.SemanticModel, context.CancellationToken, out var makeGeneric) &&
                    makeGeneric.Parameters.Length == 1 &&
                    makeGeneric.TryFindParameter("typeArguments", out var parameter) &&
                    invocation.TryFindArgument(parameter, out var argument) &&
                    GetX.TryGetType(memberAccess, context, out var type) &&
                    Array.TryGetTypes(argumentList, context, out var types))
                {
                    typeArguments = new TypeArguments(argument, type, types);
                    return true;
                }

                typeArguments = default(TypeArguments);
                return false;
            }

            public bool TryFindMisMatch(Compilation compilation, out ArgumentSyntax argument)
            {
                for (var i = 0; i < this.Types.Length; i++)
                {
                    if (!Type.SatisfiesConstraints(this.Types[i], this.GenericDefinition.TypeParameters[i], compilation))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor, argumentList.Arguments[i].GetLocation()));
                    }
                }
            }
        }
    }
}
