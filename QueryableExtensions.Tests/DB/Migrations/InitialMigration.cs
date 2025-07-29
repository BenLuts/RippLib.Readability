using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace RippLib.Readability.EFExtensions.Tests.DB.Migrations;

[DbContext(typeof(TestingDbContext))]
[Migration("Initial")]
internal class InitialMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Product",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(50)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(20)", nullable: false),
            });
    }
}
