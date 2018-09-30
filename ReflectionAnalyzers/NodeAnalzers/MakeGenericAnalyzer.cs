namespace ReflectionAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
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

            internal static bool TryCreate(InvocationExpressionSyntax invocation, QualifiedMethod expected, SyntaxNodeAnalysisContext context, out TypeArguments typeArguments)
            {
                if (invocation?.ArgumentList is ArgumentListSyntax argumentList &&
                    invocation.TryGetTarget(expected, context.SemanticModel, context.CancellationToken, out var makeGeneric) &&
                    makeGeneric.Parameters.Length == 1 &&
                    makeGeneric.TryFindParameter("typeArguments", out _) &&
                    TryGetParameters(invocation, expected, context, out var parameters))
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
