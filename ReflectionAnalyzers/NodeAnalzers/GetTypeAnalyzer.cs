namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetTypeAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL036CheckNull.Descriptor,
            REFL037TypeDoesNotExits.Descriptor,
            REFL039PreferTypeof.Descriptor);

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
                invocation.ArgumentList != null &&
                TryGetTarget(invocation, context, out var target))
            {
                if (ShouldCheckNull(invocation, target) &&
                    invocation.Parent is MemberAccessExpressionSyntax)
                {
                    context.ReportDiagnostic(Diagnostic.Create(REFL036CheckNull.Descriptor, invocation.GetLocation()));
                }

                switch (TypeExists(invocation, context, out var type, out var nameArgument))
                {
                    case true when type.IsSealed &&
                                   !type.IsAnonymousType &&
                                   target.Parameters.Length == 0 &&
                                   target == KnownSymbol.Object.GetType &&
                                   context.SemanticModel.IsAccessible(invocation.SpanStart, type):
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                REFL039PreferTypeof.Descriptor,
                                invocation.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add(
                                    nameof(TypeSyntax),
                                    type.ToString(context)),
                                type.ToString(context)));
                        break;
                    case false:
                        context.ReportDiagnostic(Diagnostic.Create(REFL037TypeDoesNotExits.Descriptor, nameArgument.GetLocation()));
                        break;
                }
            }
        }

        private static bool ShouldCheckNull(InvocationExpressionSyntax invocation, IMethodSymbol target)
        {
            if (target.Parameters.TrySingle(out var parameter) &&
                parameter.Type == KnownSymbol.String)
            {
                return true;
            }

            if (target.ReturnType == KnownSymbol.Type &&
                target.Parameters.TryFirst(out parameter) &&
                parameter.Type == KnownSymbol.String &&
                target.TryFindParameter("throwOnError", out parameter) &&
                invocation.TryFindArgument(parameter, out var arg) &&
                arg.Expression.IsKind(SyntaxKind.FalseLiteralExpression))
            {
                return true;
            }

            return false;
        }

        private static bool? TypeExists(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ITypeSymbol type, out ArgumentSyntax nameArgument)
        {
            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                nameArgument = null;
                return Type.TryGet(invocation, context, out type, out _);
            }

            if (Type.TryMatchTypeGetType(invocation, context, out var typeName, out var ignoreCase))
            {
                nameArgument = typeName.Argument;
                type = context.Compilation.GetTypeByMetadataName(typeName, ignoreCase.Value);
                return type != null;
            }

            if (Type.TryMatchAssemblyGetType(invocation, context, out typeName, out ignoreCase) &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                Assembly.TryGet(memberAccess.Expression, context, out var assembly))
            {
                nameArgument = typeName.Argument;
                type = assembly.GetTypeByMetadataName(typeName, ignoreCase.Value);
                return type != null;
            }

            nameArgument = null;
            type = null;
            return null;
        }

        private static bool TryGetTarget(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out IMethodSymbol target)
        {
            return invocation.TryGetTarget(KnownSymbol.Object.GetType, context.SemanticModel, context.CancellationToken, out target) ||
                   invocation.TryGetTarget(KnownSymbol.Type.GetType, context.SemanticModel, context.CancellationToken, out target) ||
                   invocation.TryGetTarget(KnownSymbol.Assembly.GetType, context.SemanticModel, context.CancellationToken, out target) ||
                   invocation.TryGetTarget(KnownSymbol.AssemblyBuilder.GetType, context.SemanticModel, context.CancellationToken, out target);
        }
    }
}
