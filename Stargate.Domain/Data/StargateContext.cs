using Microsoft.EntityFrameworkCore;
using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Data;

public class StargateContext : DbContext
{
    public DbSet<Person> People { get; set; } = null!;
    public DbSet<AstronautDetail> AstronautDetails { get; set; } = null!;
    public DbSet<AstronautDuty> AstronautDuties { get; set; } = null!;

    public StargateContext(DbContextOptions<StargateContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StargateContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

