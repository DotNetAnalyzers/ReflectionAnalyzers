using System;
using Gu.Roslyn.Asserts;

[assembly: CLSCompliant(false)]

[assembly: MetadataReference(typeof(object), new[] { "global", "mscorlib" })]
[assembly: MetadataReference(typeof(System.Diagnostics.Debug), new[] { "global", "System" })]
[assembly: TransitiveMetadataReferences(typeof(Microsoft.CodeAnalysis.CSharp.CSharpCompilation))]
[assembly: TransitiveMetadataReferences(typeof(Gu.Roslyn.CodeFixExtensions.DocumentEditorCodeFixProvider))]
[assembly: TransitiveMetadataReferences(typeof(System.Windows.Forms.Control))]
[assembly: TransitiveMetadataReferences(typeof(System.Windows.Controls.Control))]
[assembly: MetadataReferences(
    typeof(System.Linq.Enumerable),
    typeof(System.Reflection.Assembly),
    typeof(System.Net.WebClient),
    typeof(System.Drawing.Bitmap),
    typeof(System.Data.Common.DbConnection),
    typeof(System.Xml.Serialization.XmlSerializer),
    typeof(System.Runtime.Serialization.DataContractSerializer),
    typeof(NUnit.Framework.Assert))]
