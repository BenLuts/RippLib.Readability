using Analyzers.Test.Helpers;
using System;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Analyzers.Test;

public class ContainsAnyAsyncOverAnyAsyncTest
{
    private readonly TargetFramework _targetFramework;

    public ContainsAnyAsyncOverAnyAsyncTest()
    {
        _targetFramework = Environment.Version.Major switch
        {
            8 => TargetFramework.Net8_0,
            9 => TargetFramework.Net9_0,
            _ => TargetFramework.NetLatest
        };
    }

    [Fact]
    public async Task TriggersOnAnyAsyncWithPredicate()
    {
        var test = @"
            using System.Threading.Tasks;
            using System.Linq;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (await [|query.AnyAsync(x => x > 0)|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<ContainsAnyAsyncOverAnyAsync>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task TriggersOnAnyAsyncWithPredicateAndCancellationToken()
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
                    if (await [|query.AnyAsync(x => x > 0, ct)|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<ContainsAnyAsyncOverAnyAsync>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnAnyAsyncWithoutPredicate()
    {
        var test = @"
            using System.Threading.Tasks;
            using System.Linq;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (await query.AnyAsync())
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<ContainsAnyAsyncOverAnyAsync>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnNegatedAnyAsyncWithPredicate()
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
            .WithAnalyzer<ContainsAnyAsyncOverAnyAsync>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFix_ReplacesAnyAsyncWithContainsAnyAsync()
    {
        var testCode = @"
            using System.Linq;
            using System.Threading.Tasks;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (await [|query.AnyAsync(x => x > 0)|])
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
        if (await query.ContainsAnyAsync(x => x > 0))
        {
        }
    }
}";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<ContainsAnyAsyncOverAnyAsync>()
            .WithCodeFixProvider<CodeFixes.ContainsAnyAsyncOverAnyAsyncCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFix_ReplacesAnyAsyncWithPredicateAndCancellationToken()
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
                    if (await [|query.AnyAsync(x => x > 0, CancellationToken.None)|])
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
        if (await query.ContainsAnyAsync(x => x > 0, CancellationToken.None))
        {
        }
    }
}";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<ContainsAnyAsyncOverAnyAsync>()
            .WithCodeFixProvider<CodeFixes.ContainsAnyAsyncOverAnyAsyncCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }
}
