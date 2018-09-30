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
                TypeArguments.TryCreate(invocation, context, out var typeArguments))
            {
                if (typeArguments.Parameters.Length != typeArguments.Arguments.Length)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor, invocation.ArgumentList.GetLocation()));
                }
                else if (typeArguments.TryFindMisMatch(context, out var mismatch))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL031UseCorrectGenericArguments.Descriptor, mismatch.GetLocation()));
                }
            }
        }

        private struct TypeArguments
        {
#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
            internal readonly ImmutableArray<ITypeParameterSymbol> Parameters;
            internal readonly ImmutableArray<ExpressionSyntax> Arguments;
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

            public TypeArguments(ImmutableArray<ITypeParameterSymbol> parameters, ImmutableArray<ExpressionSyntax> arguments)
            {
                this.Parameters = parameters;
                this.Arguments = arguments;
            }

            internal static bool TryCreate(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out TypeArguments typeArguments)
            {
                if (invocation?.ArgumentList is ArgumentListSyntax argumentList &&
                    (TryGetTypeParameters(invocation, context, out var parameters) ||
                     TryGetMethodParameters(invocation, context, out parameters)))
                {
                    if (argumentList.Arguments.TrySingle(out var argument) &&
                        Array.TryGetValues(argument.Expression, context, out var arrayExpressions))
                    {
                        typeArguments = new TypeArguments(parameters, arrayExpressions);
                        return true;
                    }

                    typeArguments = new TypeArguments(parameters, Expressions());
                    return true;
                }

                typeArguments = default(TypeArguments);
                return false;

                ImmutableArray<ExpressionSyntax> Expressions()
                {
                    var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>(argumentList.Arguments.Count);
                    foreach (var arg in argumentList.Arguments)
                    {
                        builder.Add(arg.Expression);
                    }

                    return builder.ToImmutable();
                }
            }

            internal bool TryFindMisMatch(SyntaxNodeAnalysisContext context, out ExpressionSyntax argument)
            {
                for (var i = 0; i < this.Arguments.Length; i++)
                {
                    if (Type.TryGet(this.Arguments[i], context, out var type, out _) &&
                        !Type.SatisfiesConstraints(type, this.Parameters[i], context.Compilation))
                    {
                        argument = this.Arguments[i];
                        return true;
                    }
                }

                argument = null;
                return false;
            }

            private static bool TryGetTypeParameters(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ImmutableArray<ITypeParameterSymbol> parameters)
            {
                if (IsMakeGeneric(invocation, KnownSymbol.Type.MakeGenericType, context) &&
                    invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (Type.TryGet(memberAccess.Expression, context, out var type, out _))
                    {
                        parameters = (type as INamedTypeSymbol)?.TypeParameters ?? ImmutableArray<ITypeParameterSymbol>.Empty;
                        return true;
                    }

                    if (GetX.TryGetNestedType(memberAccess, context, out var nestedType))
                    {
                        parameters = nestedType.TypeParameters;
                        return true;
                    }
                }

                return false;
            }

            private static bool TryGetMethodParameters(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ImmutableArray<ITypeParameterSymbol> parameters)
            {
                if (IsMakeGeneric(invocation, KnownSymbol.MethodInfo.MakeGenericMethod, context) &&
                    invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (GetX.TryGetMethodInfo(memberAccess, context, out var methodInfo))
                    {
                        parameters = methodInfo.TypeParameters;
                        return true;
                    }
                }

                return false;
            }

            private static bool IsMakeGeneric(InvocationExpressionSyntax invocation, QualifiedMethod expected, SyntaxNodeAnalysisContext context)
            {
                return invocation.TryGetTarget(expected, context.SemanticModel, context.CancellationToken, out IMethodSymbol makeGeneric) &&
                       makeGeneric.Parameters.TrySingle(out var parameter) &&
                       parameter.IsParams &&
                       parameter.Type is IArrayTypeSymbol arrayType &&
                       arrayType.ElementType == KnownSymbol.Type;
            }
        }
    }
}
