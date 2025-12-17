using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Data.Configurations;

public class AstronautDetailConfiguration : IEntityTypeConfiguration<AstronautDetail>
{
    public void Configure(EntityTypeBuilder<AstronautDetail> builder)
    {
        builder.ToTable("AstronautDetail");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CurrentRank).HasMaxLength(50);
        builder.Property(x => x.CurrentDutyTitle).HasMaxLength(100);
        builder.Property(x => x.CareerStartDate).IsRequired();
    }
}

