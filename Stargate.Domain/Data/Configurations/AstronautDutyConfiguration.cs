using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Data.Configurations;

public class AstronautDutyConfiguration : IEntityTypeConfiguration<AstronautDuty>
{
    public void Configure(EntityTypeBuilder<AstronautDuty> builder)
    {
        builder.ToTable("AstronautDuty");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Rank).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DutyTitle).IsRequired().HasMaxLength(100);
        builder.Property(x => x.DutyStartDate).IsRequired();
    }
}

