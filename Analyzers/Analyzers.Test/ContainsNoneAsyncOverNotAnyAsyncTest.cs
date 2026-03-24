using Analyzers.Test.Helpers;
using System;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Analyzers.Test;

public class ContainsNoneAsyncOverNotAnyAsyncTest
{
    private readonly TargetFramework _targetFramework;

    public ContainsNoneAsyncOverNotAnyAsyncTest()
    {
        _targetFramework = Environment.Version.Major switch
        {
            8 => TargetFramework.Net8_0,
            9 => TargetFramework.Net9_0,
            10 => TargetFramework.Net10_0,
            _ => TargetFramework.NetLatest
        };
    }

    [Fact]
    public async Task TriggersOnNotAnyAsyncWithPredicate()
    {
        var test = @"
            using System.Threading.Tasks;
            using System.Linq;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (!await [|query.AnyAsync(x => x > 0)|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<ContainsNoneAsyncOverNotAnyAsync>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task TriggersOnNotAnyAsyncWithPredicateAndCancellationToken()
    {
        var test = @"
            using System.Linq;
            using System.Threading;
            using System.Threading.Tasks;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query, CancellationToken ct)
                {
                    if (!await [|query.AnyAsync(x => x > 0, ct)|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<ContainsNoneAsyncOverNotAnyAsync>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerWhenCompanionPackageAbsent()
    {
        var test = @"
            using System.Threading.Tasks;
            using System.Linq;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (!await query.AnyAsync(x => x > 0))
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<ContainsNoneAsyncOverNotAnyAsync>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnAnyAsyncWithPredicateWithoutNegation()
    {
        var test = @"
            using System.Threading.Tasks;
            using System.Linq;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (await query.AnyAsync(x => x > 0))
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<ContainsNoneAsyncOverNotAnyAsync>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnNotAnyAsyncWithoutPredicate()
    {
        var test = @"
            using System.Threading.Tasks;
            using System.Linq;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (!await query.AnyAsync())
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<ContainsNoneAsyncOverNotAnyAsync>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFix_ReplacesNotAnyAsyncWithContainsNoneAsync()
    {
        var testCode = @"
            using System.Linq;
            using System.Threading.Tasks;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (!await [|query.AnyAsync(x => x > 0)|])
                    {
                    }
                }
            }";

        var fixedCode = @"using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RippLib.Readability.EFExtensions;

class C
{
    async Task M(IQueryable<int> query)
    {
        if (await query.ContainsNoneAsync(x => x > 0))
        {
        }
    }
}";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<ContainsNoneAsyncOverNotAnyAsync>()
            .WithCodeFixProvider<CodeFixes.ContainsNoneAsyncOverNotAnyAsyncCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFix_ReplacesNotAnyAsyncWithPredicateAndCancellationToken()
    {
        var testCode = @"
            using System.Linq;
            using System.Threading;
            using System.Threading.Tasks;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (!await [|query.AnyAsync(x => x > 0, CancellationToken.None)|])
                    {
                    }
                }
            }";

        var fixedCode = @"using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RippLib.Readability.EFExtensions;

class C
{
    async Task M(IQueryable<int> query)
    {
        if (await query.ContainsNoneAsync(x => x > 0, CancellationToken.None))
        {
        }
    }
}";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<ContainsNoneAsyncOverNotAnyAsync>()
            .WithCodeFixProvider<CodeFixes.ContainsNoneAsyncOverNotAnyAsyncCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }
}
