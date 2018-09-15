namespace ReflectionAnalyzers
{
    using System.Collections.Immutable;
    using System.Reflection;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GetMethodAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            REFL003MemberDoesNotExist.Descriptor,
            REFL008MissingBindingFlags.Descriptor,
            REFL005WrongBindingFlags.Descriptor);

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

                invocation.ArgumentList is ArgumentListSyntax argumentList &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is TypeOfExpressionSyntax typeOf &&
                invocation.TryGetTarget(KnownSymbol.Type.GetMethod, context, out var getMethod) &&
                context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var targetType))
            {
                if (argumentList.Arguments.TryFirst(out var nameArg) &&
                    nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var targetName))
                {
                    if (targetType.TryFindFirstMethodRecursive(targetName, out var target))
                    {
                        if (targetType.TryFindSingleMethodRecursive(targetName, out target))
                        {
                            if (TryGetBindingFlags(getMethod, invocation, context, out var flagsArg, out var flags))
                            {
                                if (target.DeclaredAccessibility != Accessibility.Public &&
                                    !flags.HasFlag(BindingFlags.NonPublic))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(REFL005WrongBindingFlags.Descriptor, flagsArg.GetLocation(), targetType, targetName));
                                }

                                if (target.IsStatic &&
                                    !flags.HasFlag(BindingFlags.Static))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(REFL005WrongBindingFlags.Descriptor, flagsArg.GetLocation(), targetType, targetName));
                                }

                                if (!target.IsStatic &&
                                    !flags.HasFlag(BindingFlags.Instance))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(REFL005WrongBindingFlags.Descriptor, flagsArg.GetLocation(), targetType, targetName));
                                }
                            }
                            else
                            {
                                if (TryGetExpectedFlags(target, targetType, out var expectedFlags))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(REFL008MissingBindingFlags.Descriptor, argumentList.CloseParenToken.GetLocation(), expectedFlags.ToDisplayString()));
                                }

                                if (target.DeclaredAccessibility != Accessibility.Public)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(REFL005WrongBindingFlags.Descriptor, nameArg.GetLocation(), targetType, targetName));
                                }
                            }
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL003MemberDoesNotExist.Descriptor, nameArg.GetLocation(), targetType, targetName));
                    }
                }
            }
        }

        private static bool TryGetBindingFlags(IMethodSymbol getMethod, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ArgumentSyntax flagsArg, out BindingFlags flags)
        {
            if (getMethod.TryFindParameter(KnownSymbol.BindingFlags, out var bindingFlagsParameter) &&
                invocation.TryFindArgument(bindingFlagsParameter, out flagsArg) &&
                context.SemanticModel.TryGetConstantValue(flagsArg.Expression, context.CancellationToken, out int value))
            {
                flags = (BindingFlags)value;
                return true;
            }

            flagsArg = null;
            flags = default(BindingFlags);
            return false;
        }

        private static bool TryGetExpectedFlags(IMethodSymbol target, ITypeSymbol targetType, out BindingFlags flags)
        {
            flags = 0;
            if (target.DeclaredAccessibility == Accessibility.Public)
            {
                flags |= BindingFlags.Public;
            }
            else
            {
                flags |= BindingFlags.NonPublic;
            }

            if (target.IsStatic)
            {
                flags |= BindingFlags.Static;
            }
            else
            {
                flags |= BindingFlags.Instance;
            }

            if (Equals(target.ContainingType, targetType))
            {
                flags |= BindingFlags.DeclaredOnly;
            }
            else
            {
                flags |= BindingFlags.FlattenHierarchy;
            }

            return true;
        }
    }
}
