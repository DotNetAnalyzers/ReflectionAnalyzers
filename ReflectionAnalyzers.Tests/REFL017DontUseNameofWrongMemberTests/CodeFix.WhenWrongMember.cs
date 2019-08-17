namespace ReflectionAnalyzers.Tests.REFL017DontUseNameofWrongMemberTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class WhenWrongMember
        {
            private static readonly DiagnosticAnalyzer Analyzer = new GetXAnalyzer();
            private static readonly CodeFixProvider Fix = new NameofFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(REFL017DontUseNameofWrongMember.Descriptor);

            [Test]
            public static void TypeOfDictionaryGetMethodNameOfStaticAdd()
            {
                var before = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(↓Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";

                var after = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
                var message = "Don't use name of wrong member. Expected: Dictionary<string, object>.Add";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), before, after, fixTitle: "Use Dictionary<string, object>.Add.");
            }

            [Test]
            public static void TypeOfConsoleGetMethodNameOfStaticWriteLine()
            {
                var before = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C()
        {
            _ = typeof(Console).GetMethod(nameof(↓WriteLine), Type.EmptyTypes);
        }

        public bool WriteLine { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    using System;

    public class C
    {
        public C()
        {
            _ = typeof(Console).GetMethod(nameof(Console.WriteLine), Type.EmptyTypes);
        }

        public bool WriteLine { get; set; }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void TypeOfDictionaryGetMethodNameOfHashSetAdd()
            {
                var before = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(↓HashSet<string>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";

                var after = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void ThisGetTypeGetMethodNameOfHashSetAddStatic()
            {
                var before = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = this.GetType().GetMethod(nameof(↓HashSet<string>.Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";

                var after = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = this.GetType().GetMethod(nameof(Add));
        }

        private static int Add(int x, int y) => x + y;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void ThisGetTypeGetMethodNameOfHashSetAddInstance()
            {
                var before = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = this.GetType().GetMethod(nameof(↓HashSet<string>.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";

                var after = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = this.GetType().GetMethod(nameof(this.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void ThisGetTypeGetMethodNameOfHashSetAddUnderscore()
            {
                var c1 = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C1
    {
        private readonly int _f;
    }
}";
                var before = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = GetType().GetMethod(nameof(↓HashSet<string>.Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";

                var after = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class C
    {
        public C()
        {
            var member = GetType().GetMethod(nameof(Add));
        }

        private int Add(int x, int y) => x + y;
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after);
            }
        }
    }
}
