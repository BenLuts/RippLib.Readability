using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Analyzers.Test;
public class EmptyOverNotAnyTest
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
                    if ([|!numbers.Any()|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithAnalyzer<EmptyOverNotAny>()
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
                    if ([|!numbers.Any()|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
             .WithAnalyzer<EmptyOverNotAny>()
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
                    if ([|!numbers.Any()|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithAnalyzer<EmptyOverNotAny>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
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
             .WithAnalyzer<EmptyOverNotAny>()
             .WithSourceCode(test)
             .ShouldReportDiagnostic()
             .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnAny()
    {
        var test = @"
            using System.Linq;

            class C
            {
                void M()
                {
                    var numbers = new[] { 1, 2, 3 };
                    if (numbers.Any())
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithAnalyzer<EmptyOverNotAny>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFixReplacesNotAnyWithEmpty()
    {
        var testCode = @"
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M()
                {
                    var list = new List<int>();
                    if ([|!list.Any()|])
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
        if (list.Empty())
        {
        }
    }
}";

        await new ProjectBuilder()
            .AddRippLibReadabilityReference()
            .WithAnalyzer<EmptyOverNotAny>()
            .WithCodeFixProvider<CodeFixes.EmptyOverNotAnyCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }
}
