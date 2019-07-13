namespace ReflectionAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class NUnitAssertType : QualifiedType
    {
        internal readonly QualifiedMethod Null;
        internal readonly QualifiedMethod IsNull;
        internal readonly QualifiedMethod AreEqual;

        internal NUnitAssertType()
            : base("NUnit.Framework.Assert")
        {
            this.Null = new QualifiedMethod(this, nameof(this.Null));
            this.IsNull = new QualifiedMethod(this, nameof(this.IsNull));
            this.AreEqual = new QualifiedMethod(this, nameof(this.AreEqual));
        }
    }
}
