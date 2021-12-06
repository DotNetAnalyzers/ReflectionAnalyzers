namespace ReflectionAnalyzers.Tests
{
    using System.Runtime.CompilerServices;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;

    internal static class ModuleInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            Settings.Default = Settings.Default
                                       .WithCompilationOptions(x => x.WithNullableContextOptions(NullableContextOptions.Disable))
                                       .WithMetadataReferences(MetadataReferences.Transitive(typeof(ModuleInitializer), typeof(System.Windows.Controls.Control), typeof(System.Windows.Forms.Control)));
        }
    }
}
