using FluentAssertions;
using RippLib.Readability.EFExtensions.Tests.Bootstrapping;
using RippLib.Readability.EFExtensions.Tests.DB;
using RippLib.Readability.IQueryable;
using System;
using System.Threading.Tasks;

namespace Queryable;

public class ContainsAny : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public ContainsAny(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task WhenExistsInDB_ShouldReturn_True()
    {
        await PrepDB();
        using var context = GetContext();

        var result = await context.Products.ContainsAnyAsync(x => x.Id != Guid.Empty);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WhenNotExistsInDB_ShouldReturn_False()
    {
        await PrepDB();
        using var context = GetContext();

        var result = await context.Products.ContainsAnyAsync(x => x.Id == Guid.Empty);

        result.Should().BeFalse();
    }


    private async Task PrepDB()
    {
        await _fixture.SeedDatabase();
    }

    private TestingDbContext GetContext()
    {
        return _fixture.GetContext();
    }
}
