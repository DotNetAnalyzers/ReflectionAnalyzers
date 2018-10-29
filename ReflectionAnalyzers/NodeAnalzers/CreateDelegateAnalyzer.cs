namespace ReflectionAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class CreateDelegateAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL041CreateDelegateType.Descriptor);

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
                invocation.TryGetTarget(KnownSymbol.Delegate.CreateDelegate, QualifiedParameter.Create(KnownSymbol.Type), QualifiedParameter.Create(KnownSymbol.MethodInfo), context.SemanticModel, context.CancellationToken, out var createDelegate, out var typeArg, out var memberArg))
            {
                if (typeArg.Expression is TypeOfExpressionSyntax typeOf &&
                    context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var delegateType) &&
                    MethodInfo.TryGet(memberArg.Expression, context, out var methodInfo) &&
                    !IsCorrectDelegateType(methodInfo.Method, (INamedTypeSymbol)delegateType, context, out var delegateText))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL041CreateDelegateType.Descriptor,
                            typeOf.Type.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(nameof(TypeSyntax), delegateText)));
                }
            }
        }

        private static bool IsCorrectDelegateType(IMethodSymbol method, INamedTypeSymbol delegateType, SyntaxNodeAnalysisContext context, out string delegateText)
        {
            if (method.ReturnsVoid)
            {
                if (delegateType.MetadataName.StartsWith("Func`", StringComparison.Ordinal))
                {
                    delegateText = ActionText();
                    return false;
                }

                if (delegateType.MetadataName.StartsWith("Action", StringComparison.Ordinal))
                {
                    if (method.Parameters.Length == 0)
                    {
                        if (delegateType.TypeArguments.Length != 0)
                        {
                            delegateText = ActionText();
                            return false;
                        }
                    }
                    else if (method.Parameters.Length == delegateType.TypeArguments.Length)
                    {
                        for (var i = 0; i < method.Parameters.Length; i++)
                        {
                            if (!method.Parameters[i].Type.Equals(delegateType.TypeArguments[i]))
                            {
                                delegateText = ActionText();
                                return false;
                            }
                        }
                    }
                    else if (method.Parameters.Length != delegateType.TypeArguments.Length)
                    {
                        delegateText = ActionText();
                        return false;
                    }
                }
            }
            else
            {
                if (delegateType.MetadataName.StartsWith("Action`", StringComparison.Ordinal))
                {
                    delegateText = FuncText();
                    return false;
                }

                if (delegateType.MetadataName.StartsWith("Func`", StringComparison.Ordinal))
                {
                    if (delegateType.TypeArguments.Length == method.Parameters.Length - 1)
                    {
                        for (var i = 0; i < method.Parameters.Length; i++)
                        {
                            if (!method.Parameters[i].Type.Equals(delegateType.TypeArguments[i]))
                            {
                                delegateText = ActionText();
                                return false;
                            }
                        }

                        if (delegateType.TypeArguments.TryLast(out var last))
                        {
                            if (!method.ReturnType.Equals(last))
                            {
                                delegateText = FuncText();
                                return false;
                            }
                        }

                        delegateText = null;
                        return true;
                    }

                    delegateText = FuncText();
                    return false;
                }
            }

            delegateText = null;
            return true;

            string ActionText()
            {
                return method.Parameters.Length == 0 ? "System.Action" : $"System.Action<{string.Join(", ", method.Parameters.Select(x => x.Type.ToString(context)))}>";
            }

            string FuncText()
            {
                return method.Parameters.Length == 0 ? $"System.Func<{method.ReturnType.ToString(context)}>" : $"System.Func<{string.Join(", ", method.Parameters.Select(x => x.Type).Concat(new[] { method.ReturnType }).Select(x => x.ToString(context)))}>";
            }
        }
    }
}
