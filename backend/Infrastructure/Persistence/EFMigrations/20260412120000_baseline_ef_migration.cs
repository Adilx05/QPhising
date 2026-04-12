using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;

namespace QPhising.Infrastructure.Persistence.EFMigrations;

[DbContext(typeof(QPhisingDbContext))]
[Migration("20260412120000_baseline_ef_migration")]
public sealed class BaselineEfMigration : Migration
{
    private const string BaselineResourceName = "QPhising.Infrastructure.Persistence.LegacyMigrations.00000000000000_baseline_full_schema.sql";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var resourceStream = assembly.GetManifestResourceStream(BaselineResourceName)
            ?? throw new InvalidOperationException($"Embedded baseline SQL resource was not found: {BaselineResourceName}");

        using var reader = new StreamReader(resourceStream);
        var baselineSql = reader.ReadToEnd();
        migrationBuilder.Sql(baselineSql);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Baseline down migration is intentionally no-op.
        // Rollback should be handled by explicit incremental EF migrations going forward.
    }
}
