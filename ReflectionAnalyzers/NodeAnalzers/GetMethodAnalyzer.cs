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
            REFL003GetMethodTargetDoesNotExist.Descriptor,
            REFL005GetMethodWrongBindingFlags.Descriptor);

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
                invocation.TryGetMethodName(out var targetName) &&
                targetName == "GetMethod" &&
                invocation.ArgumentList is ArgumentListSyntax argumentList &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is TypeOfExpressionSyntax typeOf &&
                context.SemanticModel.TryGetSymbol(invocation, context.CancellationToken, out var getMethod) &&
                getMethod == KnownSymbol.Type.GetMethod &&
                context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var type))
            {
                if (argumentList.Arguments.TryFirst(out var nameArg) &&
                    nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var name))
                {
                    if (type.TryFindFirstMethodRecursive(name, out var method))
                    {
                        if (type.TryFindSingleMethodRecursive(name, out method))
                        {
                            if (TryGetBindingFlags(getMethod, invocation, context, out var flagsArg, out var flags))
                            {
                                if (method.DeclaredAccessibility != Accessibility.Public &&
                                    !flags.HasFlag(BindingFlags.NonPublic))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(REFL005GetMethodWrongBindingFlags.Descriptor, flagsArg.GetLocation(), type, name));
                                }

                                if (method.IsStatic &&
                                    !flags.HasFlag(BindingFlags.Static))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(REFL005GetMethodWrongBindingFlags.Descriptor, flagsArg.GetLocation(), type, name));
                                }

                                if (!method.IsStatic &&
                                    !flags.HasFlag(BindingFlags.Instance))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(REFL005GetMethodWrongBindingFlags.Descriptor, flagsArg.GetLocation(), type, name));
                                }
                            }
                            else if (method.DeclaredAccessibility != Accessibility.Public)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(REFL005GetMethodWrongBindingFlags.Descriptor, nameArg.GetLocation(), type, name));
                            }
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(REFL003GetMethodTargetDoesNotExist.Descriptor, nameArg.GetLocation(), type, name));
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
    }
}
