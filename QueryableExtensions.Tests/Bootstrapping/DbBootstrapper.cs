using Microsoft.EntityFrameworkCore;
using RippLib.Readability.EFExtensions.Tests.DB;

namespace RippLib.Readability.QueryableExtensions.Tests.Bootstrapping;
public sealed class DbBootstrapper
{
    private readonly DbContextOptions<TestingDbContext> _options;

    public DbBootstrapper(string connectionString)
    {
        _options = new DbContextOptionsBuilder<TestingDbContext>()
            .UseSqlServer(connectionString)
            .Options;
    }

    public TestingDbContext GetDbContext()
    {
        return new TestingDbContext(_options);
    }
}
