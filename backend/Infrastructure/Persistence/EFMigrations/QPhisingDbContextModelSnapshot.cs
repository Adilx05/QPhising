using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace QPhising.Infrastructure.Persistence.EFMigrations;

[DbContext(typeof(QPhisingDbContext))]
public partial class QPhisingDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.5")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);
#pragma warning restore 612, 618
    }
}
