using Analyzers.Test.Helpers;
using System;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Analyzers.Test;

public class EmptyAsyncOverNotAnyAsyncTest
{
    private readonly TargetFramework _targetFramework;

    public EmptyAsyncOverNotAnyAsyncTest()
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
    public async Task TriggersOnNotAnyAsyncWithoutCancellationToken()
    {
        var test = @"
            using System.Threading.Tasks;
            using System.Linq;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (!await [|query.AnyAsync()|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<EmptyAsyncOverNotAnyAsync>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task TriggersOnNotAnyAsyncWithCancellationToken()
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
                    if (!await [|query.AnyAsync(ct)|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<EmptyAsyncOverNotAnyAsync>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnAnyAsyncWithoutNegation()
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
            .WithAnalyzer<EmptyAsyncOverNotAnyAsync>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnNotAnyAsyncWithPredicate()
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
            .WithAnalyzer<EmptyAsyncOverNotAnyAsync>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFix_ReplacesNotAnyAsyncWithEmptyAsync()
    {
        var testCode = @"
            using System.Linq;
            using System.Threading.Tasks;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (!await [|query.AnyAsync()|])
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
        if (await query.EmptyAsync())
        {
        }
    }
}";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<EmptyAsyncOverNotAnyAsync>()
            .WithCodeFixProvider<CodeFixes.EmptyAsyncOverNotAnyAsyncCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFix_ReplacesNotAnyAsyncCancellationTokenWithEmptyAsyncCancellationToken()
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
                    if (!await [|query.AnyAsync(CancellationToken.None)|])
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
        if (await query.EmptyAsync(CancellationToken.None))
        {
        }
    }
}";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<EmptyAsyncOverNotAnyAsync>()
            .WithCodeFixProvider<CodeFixes.EmptyAsyncOverNotAnyAsyncCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }
}
