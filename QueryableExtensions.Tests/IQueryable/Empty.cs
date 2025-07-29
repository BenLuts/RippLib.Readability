using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RippLib.Readability;
using RippLib.Readability.EFExtensions.Tests.Bootstrapping;
using RippLib.Readability.EFExtensions.Tests.DB;
using RippLib.Readability.EFExtensions.Tests.DB.Entities;
using RippLib.Readability.IQueryable;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Queryable;

public class Empty : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public Empty(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task WhenExistsInDB_ShouldReturn_False()
    {
        await PrepDB();
        using var context = GetContext();

        var result = await context.Products.EmptyAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenNotExistsInDB_ShouldReturn_True()
    {
        await ClearDB();
        using var context = GetContext();

        var result = await context.Products.EmptyAsync();

        result.Should().BeTrue();
    }


    private async Task PrepDB()
    {
        await _fixture.SeedDatabase();
    }

    private async Task ClearDB()
    {
        using var context = GetContext();
        context.Products.RemoveRange(context.Products);
        await context.SaveChangesAsync();
    }

    private TestingDbContext GetContext()
    {
        return _fixture.GetContext();
    }
}
