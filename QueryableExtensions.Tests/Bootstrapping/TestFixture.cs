using Microsoft.EntityFrameworkCore;
using RippLib.Readability.EFExtensions.Tests.DB;
using RippLib.Readability.EFExtensions.Tests.DB.Entities;
using RippLib.Readability.QueryableExtensions.Tests.Bootstrapping;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace RippLib.Readability.EFExtensions.Tests.Bootstrapping;
public class TestFixture : IAsyncLifetime
{
    protected MsSqlContainer _dbContainer;
    protected DbBootstrapper _dbBootstrapper;

    public async Task InitializeAsync()
    {
        _dbContainer = new MsSqlBuilder()
            .Build();
        await _dbContainer.StartAsync();
        _dbBootstrapper = new(_dbContainer.GetConnectionString());

        await InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        _dbBootstrapper = null;
        await _dbContainer!.DisposeAsync();
    }

    protected async Task InitializeDatabaseAsync()
    {
        using var context = GetContext();
        await context.Database.MigrateAsync();
    }

    public TestingDbContext GetContext()
    {
        return _dbBootstrapper.GetDbContext();
    }

    public async Task SeedDatabase()
    {
        using var context = GetContext();
        var product = new Product();
        context.Products.Add(product);
        await context.SaveChangesAsync();
    }
}
