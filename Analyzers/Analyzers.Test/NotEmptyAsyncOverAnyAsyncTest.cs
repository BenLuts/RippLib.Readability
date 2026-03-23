using Analyzers.Test.Helpers;
using System;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Analyzers.Test;

public class NotEmptyAsyncOverAnyAsyncTest
{
    private readonly TargetFramework _targetFramework;

    public NotEmptyAsyncOverAnyAsyncTest()
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
    public async Task TriggersOnAnyAsyncWithoutCancellationToken()
    {
        var test = @"
            using System.Threading.Tasks;
            using System.Linq;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (await [|query.AnyAsync()|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<NotEmptyAsyncOverAnyAsync>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task TriggersOnAnyAsyncWithCancellationToken()
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
                    if (await [|query.AnyAsync(ct)|])
                    {
                    }
                }
            }";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .WithAnalyzer<NotEmptyAsyncOverAnyAsync>()
            .WithSourceCode(test)
            .ShouldReportDiagnostic()
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnNotAnyAsync()
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
            .WithAnalyzer<NotEmptyAsyncOverAnyAsync>()
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
            .WithAnalyzer<NotEmptyAsyncOverAnyAsync>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task DoesNotTriggerOnAnyAsyncWithPredicate()
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
            .WithAnalyzer<NotEmptyAsyncOverAnyAsync>()
            .WithSourceCode(test)
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFix_ReplacesAnyAsyncWithNotEmptyAsync()
    {
        var testCode = @"
            using System.Linq;
            using System.Threading.Tasks;
            using Microsoft.EntityFrameworkCore;

            class C
            {
                async Task M(IQueryable<int> query)
                {
                    if (await [|query.AnyAsync()|])
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
        if (await query.NotEmptyAsync())
        {
        }
    }
}";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<NotEmptyAsyncOverAnyAsync>()
            .WithCodeFixProvider<CodeFixes.NotEmptyAsyncOverAnyAsyncCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }

    [Fact]
    public async Task CodeFix_ReplacesAnyAsyncCancellationTokenWithNotEmptyAsyncCancellationToken()
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
                    if (await [|query.AnyAsync(CancellationToken.None)|])
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
        if (await query.NotEmptyAsync(CancellationToken.None))
        {
        }
    }
}";

        await new ProjectBuilder()
            .WithTargetFramework(_targetFramework)
            .AddEntityFramework()
            .AddRippLibReadabilityEFReference()
            .WithAnalyzer<NotEmptyAsyncOverAnyAsync>()
            .WithCodeFixProvider<CodeFixes.NotEmptyAsyncOverAnyAsyncCodeFixProvider>()
            .WithSourceCode(testCode)
            .ShouldFixCodeWith(fixedCode)
            .ValidateAsync();
    }
}

