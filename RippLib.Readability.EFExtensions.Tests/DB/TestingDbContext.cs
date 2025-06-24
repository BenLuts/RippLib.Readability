using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RippLib.Readability.EFExtensions.Tests.DB.Entities;
using System.Data.Common;
using System.Reflection;

namespace RippLib.Readability.EFExtensions.Tests.DB;
public class TestingDbContext(DbContextOptions<TestingDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }

    private static readonly DbCommandInterceptor _dbCommandInterceptor
        = new DbReadInterceptor();

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.AddInterceptors(_dbCommandInterceptor);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}

public class DbReadInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        //e.g., alter command.CommandText
        var t = command.CommandText;
        return result;
    }
}
