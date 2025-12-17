using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Person");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        
        builder.HasIndex(x => x.Name).IsUnique();
        
        builder.HasOne(z => z.AstronautDetail)
            .WithOne(z => z.Person)
            .HasForeignKey<AstronautDetail>(z => z.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(z => z.AstronautDuties)
            .WithOne(z => z.Person)
            .HasForeignKey(z => z.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

