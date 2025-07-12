using LeoGRpcApi.Api.Persistence.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace LeoGRpcApi.Api.Persistence.Util;

public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public const string SchemaName = "ninjas";
    
    public DbSet<Ninja> Ninjas { get; set; }
    public DbSet<Mission> Missions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);
        
        ConfigureNinja(modelBuilder.Entity<Ninja>());
        ConfigureMission(modelBuilder.Entity<Mission>());
    }

    private static void ConfigureMission(EntityTypeBuilder<Mission> mission)
    {
        mission.HasKey(m => m.Id);
        mission.Property(m => m.Id).ValueGeneratedOnAdd();
    }

    private static void ConfigureNinja(EntityTypeBuilder<Ninja> ninja)
    {
        ninja.HasKey(n => n.Id);
        ninja.Property(n => n.Id).ValueGeneratedOnAdd();
        ninja.HasOne(n => n.CurrentMission)
             .WithMany(m => m.AssignedNinjas)
             .HasForeignKey(n => n.CurrentMissionId)
             .OnDelete(DeleteBehavior.SetNull);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();
    }
}
