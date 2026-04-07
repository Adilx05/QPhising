using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPhising.Domain.Setup;

namespace QPhising.Infrastructure.Persistence.Configurations;

public sealed class SetupStateEntityTypeConfiguration : IEntityTypeConfiguration<SetupState>
{
    public void Configure(EntityTypeBuilder<SetupState> builder)
    {
        builder.ToTable("setup_state");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.IsCompleted)
            .IsRequired();

        builder.Property(x => x.CompletedAtUtc)
            .HasColumnType("timestamp with time zone");
    }
}
