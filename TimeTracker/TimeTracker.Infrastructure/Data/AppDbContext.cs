using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<WorkSession> WorkSessions { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ActivitySession> ActivitySessions { get; set; }
    public DbSet<PredefinedSession> PredefinedSessions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfiguracja WorkSession - ustawiamy jako typ bazowy
        modelBuilder.Entity<WorkSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Konfiguracja TPH (Table Per Hierarchy)
            entity.ToTable("WorkSessions")
                 .HasDiscriminator<string>("SessionType")
                 .HasValue<WorkSession>("WorkSession")
                 .HasValue<ActivitySession>("ActivitySession");

            entity.Property(e => e.StartTime)
                .IsRequired();

            entity.Property(e => e.EndTime)
                .IsRequired(false);

            entity.Property(e => e.Description)
                .IsRequired(false)
                .HasMaxLength(500);

            entity.Property(e => e.Tags)
                .IsRequired(false)
                .HasMaxLength(500);

            entity.HasIndex(e => new { e.StartTime, e.EndTime });
            entity.HasIndex(e => e.EndTime);
        });

        // Konfiguracja Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .IsRequired(false)
                .HasMaxLength(1000);

            entity.Property(e => e.Type)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.LastModifiedAt)
                .IsRequired(false);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
        });

        // Konfiguracja ActivitySession - konfigurujemy dodatkowe właściwości
        modelBuilder.Entity<ActivitySession>(entity =>
        {
            // Dodatkowe właściwości ActivitySession
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .IsRequired(false);

            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(e => e.Rating)
                .IsRequired(false);

            // Relacja z Project
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Sessions)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Rating);
            entity.HasIndex(e => e.ProjectId);
        });

        // Konfiguracja PredefinedSession
        modelBuilder.Entity<PredefinedSession>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .IsRequired(false)
                .HasMaxLength(500);

            entity.Property(e => e.Category)
                .IsRequired(false)
                .HasMaxLength(50);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.Category);
        });

        base.OnModelCreating(modelBuilder);
    }
} 