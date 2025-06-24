using Microsoft.EntityFrameworkCore;
using RippLib.Readability.EFExtensions.Tests.DB;
using RippLib.Readability.EFExtensions.Tests.DB.Entities;
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
        var context = GetContext();
        await context.Database.MigrateAsync();
        await context.DisposeAsync();
    }

    public TestingDbContext GetContext()
    {
        return _dbBootstrapper.GetDbContext();
    }

    public async Task SeedDatabase()
    {
        using var context = GetContext();
        context.Products.Add(new Product());
        await context.SaveChangesAsync();
    }
}
