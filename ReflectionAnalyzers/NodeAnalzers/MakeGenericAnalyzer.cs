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
                if (typeArguments.Parameters.Length != typeArguments.Arguments.Length)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor, invocation.ArgumentList.GetLocation()));
                }
                else if (typeArguments.TryFindMisMatch(context.Compilation, out var mismatch))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor, mismatch.GetLocation()));
                }
            }
        }

        private struct TypeArguments
        {
            internal readonly ArgumentListSyntax ArgumentList;
#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
            internal readonly ImmutableArray<ITypeParameterSymbol> Parameters;
            internal readonly ImmutableArray<ITypeSymbol> Arguments;
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

            public TypeArguments(ArgumentListSyntax argumentList, ImmutableArray<ITypeParameterSymbol> parameters, ImmutableArray<ITypeSymbol> arguments)
            {
                this.ArgumentList = argumentList;
                this.Parameters = parameters;
                this.Arguments = arguments;
            }

            internal static bool TryCreate(InvocationExpressionSyntax invocation, QualifiedMethod expected, SyntaxNodeAnalysisContext context, out TypeArguments typeArguments)
            {
                if (invocation?.ArgumentList is ArgumentListSyntax argumentList &&
                    invocation.TryGetTarget(expected, context.SemanticModel, context.CancellationToken, out var makeGeneric) &&
                    makeGeneric.Parameters.Length == 1 &&
                    makeGeneric.TryFindParameter("typeArguments", out _) &&
                    TryGetParameters(invocation, expected, context, out var type) &&
                    Array.TryGetTypes(argumentList, context, out var types))
                {
                    typeArguments = new TypeArguments(argumentList, type, types);
                    return true;
                }

                typeArguments = default(TypeArguments);
                return false;
            }

            internal bool TryFindMisMatch(Compilation compilation, out ArgumentSyntax argument)
            {
                for (var i = 0; i < this.Arguments.Length; i++)
                {
                    if (!Type.SatisfiesConstraints(this.Arguments[i], this.Parameters[i], compilation))
                    {
                        _ = this.ArgumentList.Arguments.TryElementAt(i, out argument);
                        return true;
                    }
                }

                argument = null;
                return false;
            }

            private static bool TryGetParameters(InvocationExpressionSyntax invocation, QualifiedMethod expected, SyntaxNodeAnalysisContext context, out ImmutableArray<ITypeParameterSymbol> parameters)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (expected.Name == KnownSymbol.Type.GetNestedType.Name &&
                        GetX.TryGetType(memberAccess, context, out var type) &&
                        type.TypeParameters.Length > 0)
                    {
                        parameters = type.TypeParameters;
                        return true;
                    }

                    if (expected.Name == KnownSymbol.MethodInfo.MakeGenericMethod.Name &&
                        GetX.TryGetMethodInfo(memberAccess, context, out var methodInfo) &&
                        methodInfo.TypeParameters.Length > 0)
                    {
                        parameters = methodInfo.TypeParameters;
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
