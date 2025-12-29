using Microsoft.EntityFrameworkCore;
using DidIDoThatApp.Models;

namespace DidIDoThatApp.Data;

/// <summary>
/// Entity Framework Core database context for the application.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Categories table.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Tasks table.
    /// </summary>
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    /// <summary>
    /// Task completion logs table.
    /// </summary>
    public DbSet<TaskLog> TaskLogs => Set<TaskLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure TaskItem
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Tasks)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TaskLog
        modelBuilder.Entity<TaskLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(e => e.TaskItem)
                .WithMany(t => t.TaskLogs)
                .HasForeignKey(e => e.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CompletedDate);
        });
    }
}
