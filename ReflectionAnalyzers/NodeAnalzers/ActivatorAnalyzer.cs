namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ActivatorAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.REFL001CastReturnValue,
            Descriptors.REFL025ArgumentsDoNotMatchParameters,
            Descriptors.REFL026NoDefaultConstructor,
            Descriptors.REFL028CastReturnValueToCorrectType);

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
                context.Node is InvocationExpressionSyntax { ArgumentList: { } } invocation &&
                invocation.TryGetTarget(KnownSymbol.Activator.CreateInstance, context.SemanticModel, context.CancellationToken, out var createInstance) &&
                TryGetCreatedType(createInstance, invocation, context, out var createdType, out var typeSource))
            {
                if (!createInstance.IsGenericMethod &&
                    ReturnValue.ShouldCast(invocation, createdType, context))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.REFL001CastReturnValue,
                            invocation.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(
                                nameof(TypeSyntax),
                                createdType.ToString(context)),
                            createdType.ToString(context)));
                }

                if (createdType is INamedTypeSymbol namedType)
                {
                    if (IsMissingDefaultConstructor(createInstance, invocation, namedType))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL026NoDefaultConstructor, typeSource.GetLocation(), createdType.ToDisplayString()));
                    }
                    else if (IsArgumentMisMatch(createInstance, invocation, namedType, context))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL025ArgumentsDoNotMatchParameters, invocation.ArgumentList.Arguments[1].GetLocation()));
                    }
                }

                if (!createInstance.IsGenericMethod &&
                    Type.IsCastToWrongType(invocation, createdType, context, out var typeSyntax))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.REFL028CastReturnValueToCorrectType,
                            typeSyntax.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(
                                nameof(TypeSyntax),
                                createdType.ToString(context)),
                            createdType.ToString(context)));
                }
            }
        }

        private static bool TryGetCreatedType(IMethodSymbol createInstance, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out ITypeSymbol? createdType, [NotNullWhen(true)] out ExpressionSyntax? typeSource)
        {
            if (createInstance.IsGenericMethod &&
                invocation.Expression is MemberAccessExpressionSyntax { Name: GenericNameSyntax { TypeArgumentList: { Arguments: { Count: 1 } typeArguments } } } &&
                typeArguments.TrySingle(out var typeArgument) &&
                context.SemanticModel.TryGetType(typeArgument, context.CancellationToken, out createdType))
            {
                typeSource = typeArgument;
                return true;
            }

            if (createInstance.TryFindParameter(KnownSymbol.Type, out var typeParameter) &&
                invocation.TryFindArgument(typeParameter, out var typeArg) &&
                Type.TryGet(typeArg.Expression, context, out createdType, out var source))
            {
                typeSource = source switch
                {
                    TypeOfExpressionSyntax typeOf => typeOf.Type,
                    _ => typeArg.Expression,
                };
                return true;
            }

            createdType = null;
            typeSource = null;
            return false;
        }

        private static bool IsMissingDefaultConstructor(IMethodSymbol createInstance, InvocationExpressionSyntax invocation, INamedTypeSymbol createdType)
        {
            if (createInstance.IsGenericMethod &&
                !HasDefaultConstructor())
            {
                return true;
            }

            if (createInstance.Parameters.TrySingle(out _) &&
                !HasDefaultConstructor())
            {
                return true;
            }

            if (createInstance.Parameters.Length == 2 &&
                createInstance.Parameters.TryElementAt(0, out var typeParameter) &&
                typeParameter.Type == KnownSymbol.Type &&
                createInstance.Parameters.TryElementAt(1, out var flagParameter) &&
                flagParameter.Type == KnownSymbol.Boolean &&
                invocation.TryFindArgument(flagParameter, out var flagArg))
            {
                switch (flagArg.Expression)
                {
                    case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.TrueLiteralExpression) &&
                                                              Type.HasVisibleNonPublicMembers(createdType, recursive: false) &&
                                                              !createdType.Constructors.TrySingle(
                                                                  x => x is { IsStatic: false, Parameters: { Length: 0 } },
                                                                  out _):
                        return true;
                    case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.FalseLiteralExpression) &&
                                                              !HasDefaultConstructor():
                        return true;
                }
            }

            return false;

            bool HasDefaultConstructor()
            {
                return createdType.Constructors.TrySingle(
                    x => x is { DeclaredAccessibility: Accessibility.Public, IsStatic: false, Parameters: { Length: 0 } },
                    out _);
            }
        }

        private static bool IsArgumentMisMatch(IMethodSymbol createInstance, InvocationExpressionSyntax invocation, INamedTypeSymbol createdType, SyntaxNodeAnalysisContext context)
        {
            if (invocation.ArgumentList is { Arguments: { } arguments } &&
                createInstance.Parameters.Length == 2 &&
                createInstance.Parameters.TryElementAt(1, out var parameter) && parameter.IsParams)
            {
                if (arguments[1].Expression?.IsKind(SyntaxKind.NullLiteralExpression) == true)
                {
                    return !createdType.Constructors.TrySingle(x => x.Parameters.TrySingle<IParameterSymbol>(out parameter) && parameter.IsParams, out _);
                }

                if (TryGetValues(arguments, 1, context.SemanticModel, context.CancellationToken, out var values) &&
                    TryFindConstructor(createdType, values, context.SemanticModel, context.CancellationToken) == false)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetValues(SeparatedSyntaxList<ArgumentSyntax> arguments, int startIndex, SemanticModel semanticModel, CancellationToken cancellationToken, out ImmutableArray<ExpressionSyntax> values)
        {
            var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>(arguments.Count - startIndex);
            switch (arguments[startIndex].Expression)
            {
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression):
                    return false;
                case { } expression when semanticModel.TryGetType(expression, cancellationToken, out var type) &&
                                         type is IArrayTypeSymbol { ElementType: { SpecialType: SpecialType.System_Object } }:
                    return Array.TryGetValues(expression, semanticModel, cancellationToken, out values);
            }

            for (var i = startIndex; i < arguments.Count; i++)
            {
                builder.Add(arguments[i].Expression);
            }

            values = builder.ToImmutable();
            return true;
        }

        private static bool? TryFindConstructor(INamedTypeSymbol type, ImmutableArray<ExpressionSyntax> values, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (type == null)
            {
                return null;
            }

            var foundMatch = false;
            foreach (var constructor in type.Constructors)
            {
                if (constructor.IsStatic)
                {
                    continue;
                }

                switch (Arguments.TryFindFirstMisMatch(constructor.Parameters, values, semanticModel, cancellationToken, out _))
                {
                    case false:
                        if (foundMatch)
                        {
                            return false;
                        }

                        foundMatch = true;
                        break;
                    case null:
                        return null;
                }
            }

            return foundMatch;
        }
    }
}
