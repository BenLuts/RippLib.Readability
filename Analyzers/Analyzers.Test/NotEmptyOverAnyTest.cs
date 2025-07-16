using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Analyzers.Test;
public class NotEmptyOverAnyTest
{
    [Fact]
    public async Task Array()
    {
        var test = @"
            using System.Linq;

            class C
            {
                void M()
                {
                    var numbers = new[] { 1, 2, 3 };
                    if ([|numbers.Any()|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithAnalyzer<NotEmptyOverAny>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task List()
    {
        var test = @"
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M()
                {
                    List<int> numbers = [1, 2, 3 ];
                    if ([|numbers.Any()|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithAnalyzer<NotEmptyOverAny>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task HashSet()
    {
        var test =
            @"
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M()
                {
                    HashSet<int> numbers = [1, 2, 3 ];
                    if ([|numbers.Any()|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithAnalyzer<NotEmptyOverAny>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnNotAny()
    {
        var test = @"
            using System.Linq;

            class C
            {
                void M()
                {
                    var numbers = new[] { 1, 2, 3 };
                    if (!numbers.Any())
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithAnalyzer<NotEmptyOverAny>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnOtherMethods()
    {
        var test = @"
            using System.Linq;

            class C
            {
                void M()
                {
                    var numbers = new[] { 1, 2, 3 };
                    var count = numbers.Count();
                }
            }";

        await new ProjectBuilder()
            .WithAnalyzer<NotEmptyOverAny>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnAnyWithPredicate()
    {
        var test = @"
            using System.Linq;

            class C
            {
                void M()
                {
                    var numbers = new[] { 1, 2, 3 };
                    if (numbers.Any(x => x > 0))
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithAnalyzer<NotEmptyOverAny>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnAnyWithMethod()
    {
        var test = @"
            using System.Linq;

            class C
            {
                void M()
                {
                    var numbers = new[] { 1, 2, 3 };
                    if (numbers.Any(IsPositive))
                    {
                    }
                }

                bool IsPositive(int x) => x > 0;
            }";

        await new ProjectBuilder()
            .WithAnalyzer<NotEmptyOverAny>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFixReplacesAnyWithNotEmpty()
    {
        var testCode = @"
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M()
                {
                    var list = new List<int>();
                    if ([|list.Any()|])
                    {
                    }
                }
            }";

        var fixedCode = @"using System.Collections.Generic;
using System.Linq;
using RippLib.Readability;

class C
{
    void M()
    {
        var list = new List<int>();
        if (list.NotEmpty())
        {
        }
    }
}";

        await new ProjectBuilder()
            .AddRippLibReadabilityReference()
            .WithAnalyzer<NotEmptyOverAny>()
            .WithCodeFixProvider<CodeFixes.NotEmptyOverAnyCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }
}
