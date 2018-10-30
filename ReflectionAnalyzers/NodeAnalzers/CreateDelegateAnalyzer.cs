namespace ReflectionAnalyzers
{
    using System;
    using System.Collections.Immutable;
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
                invocation.TryGetTarget(KnownSymbol.Delegate.CreateDelegate, context.SemanticModel, context.CancellationToken, out var createDelegate) &&
                TryFindArgument("type", out var typeArg) &&
                TryFindArgument("method", out var methodArg) &&
                typeArg.Expression is TypeOfExpressionSyntax typeOf &&
                context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var delegateType) &&
                MethodInfo.TryGet(methodArg.Expression, context, out var methodInfo))
            {
                var types = new DelegateTypes(methodInfo.Method, createDelegate.TryFindParameter("firstArgument", out _) ? 1 : 0);
                if (!IsCorrectDelegateType(types, (INamedTypeSymbol)delegateType, context, out var delegateText))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL041CreateDelegateType.Descriptor,
                            typeOf.Type.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(nameof(TypeSyntax), delegateText),
                            delegateText));
                }
            }

            bool TryFindArgument(string name, out ArgumentSyntax argument)
            {
                argument = null;
                return createDelegate.TryFindParameter(name, out var parameter) &&
                       invocation.TryFindArgument(parameter, out argument);
            }
        }

        private static bool IsCorrectDelegateType(DelegateTypes types, INamedTypeSymbol delegateType, SyntaxNodeAnalysisContext context, out string delegateText)
        {
            if (types.ReturnType.SpecialType == SpecialType.System_Void)
            {
                if (delegateType.MetadataName.StartsWith("Func`", StringComparison.Ordinal))
                {
                    delegateText = ActionText();
                    return false;
                }

                if (delegateType.MetadataName.StartsWith("Action", StringComparison.Ordinal))
                {
                    if (types.Count == delegateType.TypeArguments.Length)
                    {
                        for (var i = 0; i < types.Count; i++)
                        {
                            if (!types[i].Equals(delegateType.TypeArguments[i]))
                            {
                                delegateText = ActionText();
                                return false;
                            }
                        }

                        delegateText = null;
                        return true;
                    }

                    delegateText = ActionText();
                    return false;
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
                    if (types.Count == delegateType.TypeArguments.Length)
                    {
                        for (var i = 0; i < types.Count; i++)
                        {
                            if (!types[i].Equals(delegateType.TypeArguments[i]))
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
                return types.Count == 0 ?
                    "System.Action" :
                    $"System.Action<{types.TypeArgsText(context)}>";
            }

            string FuncText()
            {
                return $"System.Func<{types.TypeArgsText(context)}>";
            }
        }

        private struct DelegateTypes
        {
            private readonly IMethodSymbol method;
            private readonly int startIndex;

            public DelegateTypes(IMethodSymbol method, int startIndex)
            {
                this.method = method;
                this.startIndex = startIndex;
            }

            public int Count
            {
                get
                {
                    var count = this.method.Parameters.Length - this.startIndex;
                    if (!this.method.ReturnsVoid)
                    {
                        count++;
                    }

                    if (!this.method.IsStatic)
                    {
                        count++;
                    }

                    return count;
                }
            }

            public ITypeSymbol ReturnType => this.method.ReturnType;

            public ITypeSymbol this[int index]
            {
                get
                {
                    if (!this.method.IsStatic)
                    {
                        index--;
                    }

                    index += this.startIndex;
                    if (index == -1)
                    {
                        return this.method.ContainingType;
                    }

                    if (index < this.method.Parameters.Length)
                    {
                        return this.method.Parameters[index].Type;
                    }

                    if (index == this.method.Parameters.Length &&
                       !this.method.ReturnsVoid)
                    {
                        return this.method.ReturnType;
                    }

                    throw new ArgumentOutOfRangeException(nameof(index), index, "DelegateTypes[] should never get here, bug in the analyzer.");
                }
            }

            public string TypeArgsText(SyntaxNodeAnalysisContext context)
            {
                var builder = StringBuilderPool.Borrow();
                for (var i = 0; i < this.Count; i++)
                {
                    if (i > 0)
                    {
                        _ = builder.Append(", ");
                    }

                    _ = builder.Append(this[i].ToString(context));
                }

                return builder.Return();
            }
        }
    }
}
