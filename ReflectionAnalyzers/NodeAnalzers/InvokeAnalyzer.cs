namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class InvokeAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL001CastReturnValue.Descriptor,
            REFL002DiscardReturnValue.Descriptor,
            REFL024PreferNullOverEmptyArray.Descriptor,
            REFL025ArgumentsDontMatchParameters.Descriptor,
            REFL028CastReturnValueToCorrectType.Descriptor,
            REFL030UseCorrectObj.Descriptor,
            REFL035DontInvokeGenericDefinition.Descriptor,
            REFL038PreferRunClassConstructor.Descriptor);

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
                context.Node is InvocationExpressionSyntax { ArgumentList: { }, Expression: MemberAccessExpressionSyntax memberAccess } invocation &&
                invocation.TryGetMethodName(out var name) &&
                name == "Invoke" &&
                context.SemanticModel.TryGetSymbol(invocation, context.CancellationToken, out var invoke) &&
                invoke.ContainingType.IsAssignableTo(KnownSymbol.MemberInfo, context.Compilation) &&
                invoke.TryFindParameter("parameters", out var parametersParameter) &&
                invocation.TryFindArgument(parametersParameter, out var parametersArg))
            {
                if (context.SemanticModel.TryGetType(parametersArg.Expression, context.CancellationToken, out var type) &&
                    type is IArrayTypeSymbol arrayType &&
                    arrayType.ElementType == KnownSymbol.Object &&
                    Array.IsCreatingEmpty(parametersArg.Expression, context))
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL024PreferNullOverEmptyArray.Descriptor, parametersArg.GetLocation()));
                }

                if (GetX.TryGetMethodInfo(memberAccess, context, out var method))
                {
                    if (!method.ReturnsVoid &&
                        ReturnValue.ShouldCast(invocation, method.ReturnType, context))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL001CastReturnValue.Descriptor,
                                invocation.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(TypeSyntax),
                                    method.ReturnType.ToString(context)),
                                method.ReturnType.ToString(context)));
                    }

                    if (method.ReturnsVoid &&
                        !IsResultDiscarded(invocation, context))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL002DiscardReturnValue.Descriptor, invocation.GetLocation()));
                    }

                    if (Array.TryGetValues(parametersArg.Expression, context, out var values) &&
                        Arguments.TryFindFirstMisMatch(method.Parameters, values, context, out var misMatch) == true)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor, misMatch?.GetLocation() ?? parametersArg.GetLocation()));
                    }

                    if (parametersArg.Expression.IsKind(SyntaxKind.NullLiteralExpression) &&
                        method.Parameters.Length > 0)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor, parametersArg.GetLocation()));
                    }

                    if (Type.IsCastToWrongType(invocation, method.ReturnType, context, out var castType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL028CastReturnValueToCorrectType.Descriptor,
                                castType.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(TypeSyntax),
                                    method.ReturnType.ToString(context)),
                                method.ReturnType.ToString(context)));
                    }

                    if (invoke.TryFindParameter("obj", out var objParameter) &&
                        invocation.TryFindArgument(objParameter, out var objArg))
                    {
                        if (method.IsStatic &&
                            objArg.Expression?.IsKind(SyntaxKind.NullLiteralExpression) == false)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(REFL030UseCorrectObj.Descriptor, objArg.GetLocation(), $"The method {method} is static and null should be passed as obj."));
                        }

                        if (!method.IsStatic)
                        {
                            if (objArg.Expression?.IsKind(SyntaxKind.NullLiteralExpression) == true)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(REFL030UseCorrectObj.Descriptor, objArg.GetLocation(), $"The method {method} is an instance method and the instance should be passed as obj."));
                            }

                            if (context.SemanticModel.TryGetType(objArg.Expression, context.CancellationToken, out var objType) &&
                                objType != KnownSymbol.Object &&
                                !objType.IsAssignableTo(method.ContainingType, context.Compilation))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(REFL030UseCorrectObj.Descriptor, objArg.GetLocation(), $"Expected an argument of type {method.ContainingType}."));
                            }
                        }
                    }

                    if (method.IsGenericDefinition())
                    {
                        if (values != null &&
                            values.Length > 0 &&
                            Array.TryGetAccessibleTypes(values, context, out var types))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL035DontInvokeGenericDefinition.Descriptor,
                                    invocation.GetNameLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(
                                        nameof(TypeSyntax),
                                        string.Join(", ", types.Select(x => $"typeof({x.ToString(context)})")))));
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(REFL035DontInvokeGenericDefinition.Descriptor, invocation.GetNameLocation()));
                        }
                    }
                }
                else if (GetX.TryGetConstructorInfo(memberAccess, context, out var ctor))
                {
                    if (ReturnValue.ShouldCast(invocation, ctor.ReturnType, context))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL001CastReturnValue.Descriptor,
                                invocation.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(TypeSyntax),
                                    ctor.ContainingType.ToString(context)),
                                ctor.ContainingType.ToString(context)));
                    }

                    if (Array.TryGetValues(parametersArg.Expression, context, out var values) &&
                        Arguments.TryFindFirstMisMatch(ctor.Parameters, values, context, out var misMatch) == true)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL025ArgumentsDontMatchParameters.Descriptor, misMatch?.GetLocation() ?? parametersArg.GetLocation()));
                    }

                    if (invoke.TryFindParameter("obj", out var objParameter) &&
                        invocation.TryFindArgument(objParameter, out var objArg))
                    {
                        if (objArg.Expression.IsKind(SyntaxKind.NullLiteralExpression))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(REFL030UseCorrectObj.Descriptor, objArg.GetLocation(), "Use overload of Invoke without obj parameter."));
                        }
                        else
                        {
                            if (!IsResultDiscarded(invocation, context))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(REFL002DiscardReturnValue.Descriptor, invocation.GetLocation()));
                            }

                            if (!context.SemanticModel.TryGetType(objArg.Expression, context.CancellationToken, out var instanceType) ||
                                (instanceType != KnownSymbol.Object && !instanceType.Equals(ctor.ContainingType)))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(REFL030UseCorrectObj.Descriptor, objArg.GetLocation(), $"Use an instance of type {ctor.ContainingType}."));
                            }
                        }
                    }
                    else
                    {
                        if (!ctor.IsStatic &&
                            Type.IsCastToWrongType(invocation, ctor.ContainingType, context, out var castType))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    REFL028CastReturnValueToCorrectType.Descriptor,
                                    castType.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(
                                        nameof(TypeSyntax),
                                        ctor.ContainingType.ToString(context)),
                                    ctor.ContainingType.ToString(context)));
                        }
                    }

                    if (ctor.IsStatic)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL038PreferRunClassConstructor.Descriptor,
                                invocation.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(TypeSyntax),
                                    ctor.ContainingType.ToMinimalDisplayString(context.SemanticModel, invocation.SpanStart))));
                    }
                }
            }
        }

        private static bool IsResultDiscarded(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
        {
            switch (invocation.Parent)
            {
                case ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax candidate } }
                    when IsAssert(candidate):
                    return true;
                case ArgumentSyntax _:
                case MemberAccessExpressionSyntax _:
                case CastExpressionSyntax _:
                case BinaryExpressionSyntax _:
                    return false;
                case AssignmentExpressionSyntax assignment:
                    return assignment.Left is IdentifierNameSyntax identifierName && IsDiscardName(identifierName.Identifier.ValueText);
                case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax variableDeclarator }:
                    return IsDiscardName(variableDeclarator.Identifier.ValueText);
                default:
                    return true;
            }

            bool IsAssert(InvocationExpressionSyntax candidate)
            {
                return candidate.TryGetTarget(KnownSymbol.NUnitAssert.Null, context.SemanticModel, context.CancellationToken, out _) ||
                       candidate.TryGetTarget(KnownSymbol.NUnitAssert.IsNull, context.SemanticModel, context.CancellationToken, out _) ||
                       candidate.TryGetTarget(KnownSymbol.NUnitAssert.AreEqual, context.SemanticModel, context.CancellationToken, out _);
            }
        }

        private static bool IsDiscardName(string text)
        {
            foreach (var c in text)
            {
                if (c != '_')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
