using Gu.Roslyn.Asserts;

[assembly: MetadataReference(typeof(object), new[] { "global", "mscorlib" })]
[assembly: MetadataReference(typeof(System.Diagnostics.Debug), new[] { "global", "System" })]
[assembly: TransitiveMetadataReferences(typeof(Microsoft.CodeAnalysis.CSharp.CSharpCompilation))]
[assembly: TransitiveMetadataReferences(typeof(Gu.Roslyn.CodeFixExtensions.DocumentEditorCodeFixProvider))]
[assembly: MetadataReferences(
    typeof(System.Linq.Enumerable),
    typeof(System.Reflection.Assembly),
    typeof(System.Net.WebClient),
    typeof(System.Drawing.Bitmap),
    typeof(System.Data.Common.DbConnection),
    typeof(System.Xml.Serialization.XmlSerializer),
    typeof(System.Runtime.Serialization.DataContractSerializer),
    typeof(NUnit.Framework.Assert))]
