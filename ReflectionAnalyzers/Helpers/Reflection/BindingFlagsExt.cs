namespace ReflectionAnalyzers;

using System;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class BindingFlagsExt
{
    internal static bool HasEither(this BindingFlags flags, BindingFlags flag1, BindingFlags flag2) => HasFlagFast(flags, flag1) || HasFlagFast(flags, flag2);

    internal static bool HasFlagFast(this BindingFlags flags, BindingFlags flag)
    {
        if ((int)flags == -1)
        {
            return false;
        }

        return (flags == BindingFlags.Default && flag == BindingFlags.Default) ||
               (flags & flag) != 0;
    }

    internal static string ToDisplayString(this BindingFlags flags, SyntaxNode? location)
    {
        var usingStatic = IsUsingStatic(location);
        var builder = StringBuilderPool.Borrow();
        //// below is in specific order.
        AppendIf(BindingFlags.Public);
        AppendIf(BindingFlags.NonPublic);
        AppendIf(BindingFlags.Static);
        AppendIf(BindingFlags.Instance);
        AppendIf(BindingFlags.DeclaredOnly);
        AppendIf(BindingFlags.FlattenHierarchy);

        // below is in no specific order
        AppendIf(BindingFlags.CreateInstance);
        AppendIf(BindingFlags.Default);
        AppendIf(BindingFlags.ExactBinding);
        AppendIf(BindingFlags.GetField);
        AppendIf(BindingFlags.GetProperty);
        AppendIf(BindingFlags.IgnoreCase);
        AppendIf(BindingFlags.InvokeMethod);
        AppendIf(BindingFlags.IgnoreReturn);
        AppendIf(BindingFlags.OptionalParamBinding);
        AppendIf(BindingFlags.PutDispProperty);
        AppendIf(BindingFlags.PutRefDispProperty);
        AppendIf(BindingFlags.SetField);
        AppendIf(BindingFlags.SetProperty);
        AppendIf(BindingFlags.SuppressChangeType);
        AppendIf(BindingFlags.DoNotWrapExceptions);

        return builder.Return();
        void AppendIf(BindingFlags flag)
        {
            if (flags.HasFlagFast(flag))
            {
                if (builder.Length != 0)
                {
                    _ = builder.Append(" | ");
                }

                if (!usingStatic)
                {
                    _ = builder.Append("BindingFlags.");
                }

                _ = builder.Append(flag.Name());
            }
        }
    }

    private static bool IsUsingStatic(SyntaxNode? location)
    {
        if (location is null)
        {
            return false;
        }

        if (location.TryFirstAncestor(out NamespaceDeclarationSyntax? namespaceDeclaration))
        {
            return namespaceDeclaration.Usings.TryFirst(x => IsBindingFlags(x), out _) ||
                   (namespaceDeclaration.Parent is CompilationUnitSyntax compilationUnit &&
                    compilationUnit.Usings.TryFirst(x => IsBindingFlags(x), out _));
        }

        return false;

        static bool IsBindingFlags(UsingDirectiveSyntax @using)
        {
            return @using.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) &&
                   @using.Name is QualifiedNameSyntax qn &&
                   qn.Right.Identifier.ValueText == "BindingFlags";
        }
    }

    private static string Name(this BindingFlags flag)
    {
        return flag switch
        {
            BindingFlags.CreateInstance => "CreateInstance",
            BindingFlags.Default => "Default",
            BindingFlags.DeclaredOnly => "DeclaredOnly",
            BindingFlags.ExactBinding => "ExactBinding",
            BindingFlags.FlattenHierarchy => "FlattenHierarchy",
            BindingFlags.GetField => "GetField",
            BindingFlags.GetProperty => "GetProperty",
            BindingFlags.IgnoreCase => "IgnoreCase",
            BindingFlags.IgnoreReturn => "IgnoreReturn",
            BindingFlags.Instance => "Instance",
            BindingFlags.InvokeMethod => "InvokeMethod",
            BindingFlags.NonPublic => "NonPublic",
            BindingFlags.OptionalParamBinding => "OptionalParamBinding",
            BindingFlags.PutDispProperty => "PutDispProperty",
            BindingFlags.PutRefDispProperty => "PutRefDispProperty",
            BindingFlags.Public => "Public",
            BindingFlags.SetField => "SetField",
            BindingFlags.SetProperty => "SetProperty",
            BindingFlags.Static => "Static",
            BindingFlags.SuppressChangeType => "SuppressChangeType",
            BindingFlags.DoNotWrapExceptions => "DoNotWrapExceptions",
            _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, null),
        };
    }
}
