﻿namespace ReflectionAnalyzers;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class DefaultMemberAttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.REFL046DefaultMemberMustExist);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new System.ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Attribute);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (context.IsExcludedFromAnalysis())
        {
            return;
        }

        if (!TryGetAttributeAndTypeInfo(context, out var memberName, out var location, out var symbol))
        {
            return;
        }

        if (!IsInvokableMember(symbol, memberName))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.REFL046DefaultMemberMustExist, location));
        }
    }

    /// <summary>
    /// Attempt to retrieve information about the application of the
    /// DefaultMemberAttribute:
    ///
    ///  - Specified member name
    ///  - Location of attribute
    ///  - Type symbol of type with attribute applied
    ///
    /// Does not throw on failure.
    /// </summary>
    /// <param name="context">Context of attribute application.</param>
    /// <param name="memberName">Member name specified by attribute.</param>
    /// <param name="location">Location of attribute application.</param>
    /// <param name="typeSymbol">Attribute target.</param>
    /// <returns>Success.</returns>
    private static bool TryGetAttributeAndTypeInfo(
        SyntaxNodeAnalysisContext context,
        [NotNullWhen(true)] out string? memberName,
        [NotNullWhen(true)] out Location? location,
        [NotNullWhen(true)] out ITypeSymbol? typeSymbol)
    {
        memberName = default;
        location = default;
        typeSymbol = default;

        if (context.Node is not AttributeSyntax attribute ||
            !context.SemanticModel.TryGetNamedType(attribute, KnownSymbol.DefaultMemberAttribute, context.CancellationToken, out _))
        {
            return false;
        }

        if (!attribute.TryFindArgument(0, "memberName", out var argument) ||
            !context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string? workingMemberName))
        {
            return false;
        }

        if (!attribute.TryFirstAncestor(out ClassDeclarationSyntax? classDeclaration))
        {
            return false;
        }

        if (!context.SemanticModel.TryGetSymbol(classDeclaration, context.CancellationToken, out var workingTypeSymbol))
        {
            return false;
        }

        if (workingMemberName is null)
        {
            return false;
        }

        memberName = workingMemberName;
        location = argument.GetLocation();
        typeSymbol = workingTypeSymbol;
        return true;
    }

    /// <summary>
    /// Check if Type.InvokeMember(memberName, ...) will work with the type
    /// defined by `symbol` and the supplied member name, assuming the
    /// other arguments are correct.
    /// </summary>
    /// <param name="symbol">symbol of type to call InvokeMember on.</param>
    /// <param name="memberName">
    /// Name to be implicitly passed in to InvokeMember when `name`
    /// argument is an empty string.
    /// </param>
    /// <returns>whether the supplied member name is valid for the type.</returns>
    private static bool IsInvokableMember(ITypeSymbol symbol, string memberName)
    {
        // invoke is not supported with base class constructors on derived
        // types, so we should only check for a constructor match on the
        // type which actually has the DefaultMember attribute.
        if (symbol.Name == memberName)
        {
            return true;
        }

        return IsInvokableNonConstructor(symbol, memberName);
    }

    /// <summary>
    /// Check if Type.InvokeMember(memberName, ...) will work with the type
    /// defined by `symbol` and the supplied member name, assuming the
    /// other arguments are correct, and the invokeAttr does not contain
    /// BindingFlags.CreateInstace.
    /// </summary>
    /// <param name="symbol">symbol of type to call InvokeMember on.</param>
    /// <param name="memberName">
    /// Name to be implicitly passed in to InvokeMember when `name`
    /// argument is an empty string.
    /// </param>
    /// <returns>whether the supplied member name is valid for the type.</returns>
    private static bool IsInvokableNonConstructor(ITypeSymbol symbol, string memberName)
    {
        var matches = symbol.GetMembers(memberName);

        if (matches.Any(IsInvokableMember))
        {
            return true;
        }

        if (symbol.BaseType is null)
        {
            return false;
        }

        return IsInvokableNonConstructor(symbol.BaseType, memberName);

        static bool IsInvokableMember(ISymbol member)
        {
            return
                member is IFieldSymbol ||
                member is IPropertySymbol ||
                member is IMethodSymbol;
        }
    }
}
