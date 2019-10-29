namespace ReflectionAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
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
            REFL001CastReturnValue.Descriptor,
            REFL028CastReturnValueToCorrectType.Descriptor,
            REFL041CreateDelegateType.Descriptor,
            REFL042FirstArgumentIsReferenceType.Descriptor,
            REFL043FirstArgumentType.Descriptor);

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
                MethodInfo.TryGet(methodArg.Expression, context, out var methodInfo) &&
                typeArg.Expression is TypeOfExpressionSyntax typeOf &&
                context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var delegateType))
            {
                if (ReturnValue.ShouldCast(invocation, delegateType, context))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            REFL001CastReturnValue.Descriptor,
                            invocation.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(
                                nameof(TypeSyntax),
                                delegateType.ToString(context)),
                            delegateType.ToString(context)));
                }

                if (TryFindDelegateMethod(out var delegateMethod) &&
                    new MethodTypes(methodInfo, createDelegate.TryFindParameter("firstArgument", out _)) is var argumentTypes)
                {
                    if (!IsCorrectDelegateType(argumentTypes, delegateMethod, context, out var delegateText))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL041CreateDelegateType.Descriptor,
                                typeOf.Type.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(nameof(TypeSyntax), delegateText),
                                delegateText));
                    }
                    else if (Type.IsCastToWrongType(invocation, delegateType, context, out var typeSyntax))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL028CastReturnValueToCorrectType.Descriptor,
                                typeSyntax.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(TypeSyntax),
                                    delegateType.ToString(context)),
                                delegateType.ToString(context)));
                    }

                    if (TryFindArgument("firstArgument", out var firstArg) &&
                        TryGetFirstArgType(methodInfo, out var firstArgType))
                    {
                        if (!firstArgType.IsReferenceType)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL042FirstArgumentIsReferenceType.Descriptor,
                                    firstArg.GetLocation()));
                        }

                        if (context.SemanticModel.TryGetType(firstArg.Expression, context.CancellationToken, out var firstArgActualType) &&
                            !firstArgActualType.IsAssignableTo(firstArgType, context.Compilation))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL043FirstArgumentType.Descriptor,
                                    firstArg.GetLocation(),
                                    firstArgType));
                        }
                    }
                }
            }

            bool TryFindArgument(string name, out ArgumentSyntax argument)
            {
                argument = null;
                return createDelegate.TryFindParameter(name, out var parameter) &&
                       invocation.TryFindArgument(parameter, out argument);
            }

            bool TryFindDelegateMethod(out IMethodSymbol delegateMethod)
            {
                if (delegateType is INamedTypeSymbol namedType &&
                    namedType.DelegateInvokeMethod is IMethodSymbol temp)
                {
                    delegateMethod = temp;
                    return true;
                }

                delegateMethod = null;
                return false;
            }
        }

        private static bool TryGetFirstArgType(MethodInfo methodInfo, [NotNullWhen(true)] out ITypeSymbol? type)
        {
            if (!methodInfo.Method.IsStatic)
            {
                type = methodInfo.Method.ContainingType;
                return true;
            }

            if (methodInfo.Method.Parameters.TryFirst(out var parameter))
            {
                type = parameter.Type;
                return true;
            }

            type = null;
            return false;
        }

        private static bool IsCorrectDelegateType(MethodTypes methodTypes, IMethodSymbol delegateMethod, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out string? delegateText)
        {
            if (!methodTypes.ReturnType.Equals(delegateMethod.ReturnType))
            {
                delegateText = DelegateText();
                return false;
            }

            if (methodTypes.Count == delegateMethod.Parameters.Length)
            {
                for (var i = 0; i < delegateMethod.Parameters.Length; i++)
                {
                    if (!methodTypes[i].Equals(delegateMethod.Parameters[i].Type))
                    {
                        delegateText = DelegateText();
                        return false;
                    }
                }

                delegateText = null;
                return true;
            }

            delegateText = DelegateText();
            return false;

            string DelegateText()
            {
                if (methodTypes.ReturnType.SpecialType == SpecialType.System_Void)
                {
                    if (methodTypes.Count == 0)
                    {
                        return "System.Action";
                    }

                    return StringBuilderPool.Borrow()
                                             .Append("System.Action<")
                                             .Append(methodTypes.TypeArgsText(context))
                                             .Append(">")
                                             .Return();
                }

                return StringBuilderPool.Borrow()
                                        .Append("System.Func<")
                                        .Append(methodTypes.TypeArgsText(context))
                                        .Append(">")
                                        .Return();
            }
        }

        private struct MethodTypes
        {
#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer. BUG IN THE ANALYZER
            private readonly IMethodSymbol method;
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
            private readonly int startIndex;

            internal MethodTypes(MethodInfo methodInfo, bool isCurried)
            {
                this.method = methodInfo.Method;
                this.startIndex = isCurried ? 1 : 0;
            }

            internal int Count
            {
                get
                {
                    var count = this.method.Parameters.Length - this.startIndex;
                    if (!this.method.IsStatic)
                    {
                        count++;
                    }

                    return count;
                }
            }

            internal ITypeSymbol ReturnType => this.method.ReturnType;

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

                    throw new ArgumentOutOfRangeException(nameof(index), index, "DelegateTypes[] should never get here, bug in the analyzer.");
                }
            }

            internal string TypeArgsText(SyntaxNodeAnalysisContext context)
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

                if (!this.method.ReturnsVoid)
                {
                    if (builder.Length > 0)
                    {
                        _ = builder.Append(", ");
                    }

                    _ = builder.Append(this.method.ReturnType.ToString(context));
                }

                return builder.Return();
            }
        }
    }
}
