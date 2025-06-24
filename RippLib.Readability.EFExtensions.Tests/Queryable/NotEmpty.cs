using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RippLib.Readability;
using RippLib.Readability.EFExtensions.Tests.Bootstrapping;
using RippLib.Readability.EFExtensions.Tests.DB;
using RippLib.Readability.EFExtensions.Tests.DB.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Queryable;

public class NotEmpty : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public NotEmpty(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task WhenExistsInDB_ShouldReturn_True()
    {
        await PrepDB();
        using var context = GetContext();

        var result = await context.Products.NotEmptyAsync();

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
