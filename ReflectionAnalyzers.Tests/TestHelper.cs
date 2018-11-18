namespace ReflectionAnalyzers.Tests
{
    using System.IO;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class TestHelper
    {
        public static MetadataReference CompileBinaryReference(string code)
        {
            var binaryReferencedCompilation = CSharpCompilation.Create(
                "BinaryReferencedAssembly",
                new[] { CSharpSyntaxTree.ParseText(code) },
                AnalyzerAssert.MetadataReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var binaryReferencedContent = new MemoryStream())
            {
                var binaryEmitResult = binaryReferencedCompilation.Emit(binaryReferencedContent);
                Assert.That(binaryEmitResult.Diagnostics, Is.Empty);

                binaryReferencedContent.Position = 0;
                return MetadataReference.CreateFromStream(binaryReferencedContent);
            }
        }
    }
}
