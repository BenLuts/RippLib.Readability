using FluentAssertions;
using RippLib.Readability.EFExtensions.Tests.Bootstrapping;
using RippLib.Readability.EFExtensions.Tests.DB;
using RippLib.Readability.EFExtensions;
using System;
using System.Threading.Tasks;

namespace Queryable;

public class ContainsNone : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public ContainsNone(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task WhenExistsInDB_ShouldReturn_False()
    {
        await PrepDB();
        using var context = GetContext();

        var result = await context.Products.ContainsNoneAsync(x => x.Id != Guid.Empty);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenNotExistsInDB_ShouldReturn_True()
    {
        await PrepDB();
        using var context = GetContext();

        var result = await context.Products.ContainsNoneAsync(x => x.Id == Guid.Empty);

        result.Should().BeTrue();
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
