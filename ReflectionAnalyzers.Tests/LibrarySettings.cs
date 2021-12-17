﻿namespace ReflectionAnalyzers.Tests
{
    using System.Runtime.CompilerServices;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;

    internal static class LibrarySettings
    {
        internal static readonly Settings Wpf = Settings.Default.WithMetadataReferences(MetadataReferences.Transitive(typeof(System.Windows.Controls.Control)));

        internal static readonly Settings WindowsForms = Settings.Default.WithMetadataReferences(MetadataReferences.Transitive(typeof(System.Windows.Forms.Control)));

        internal static readonly Settings NullableEnabled = Settings.Default.WithCompilationOptions(x => x.WithNullableContextOptions(NullableContextOptions.Enable));

        [ModuleInitializer]
        internal static void Initialize()
        {
            Settings.Default = Settings.Default
                                       .WithCompilationOptions(x => x.WithNullableContextOptions(NullableContextOptions.Disable))
                                       .WithMetadataReferences(MetadataReferences.Transitive(typeof(LibrarySettings), typeof(System.Windows.Controls.Control), typeof(System.Windows.Forms.Control)));
        }
    }
}
