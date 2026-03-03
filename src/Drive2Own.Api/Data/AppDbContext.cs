using Drive2Own.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Drive2Own.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<LocationPin> LocationPins => Set<LocationPin>();
    public DbSet<Models.Route> Routes => Set<Models.Route>();
    public DbSet<RoutePoint> RoutePoints => Set<RoutePoint>();
    public DbSet<RouteShare> RouteShares => Set<RouteShare>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Models.Route>(entity =>
        {
            entity.HasOne(r => r.User)
                .WithMany(u => u.Routes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoutePoint>(entity =>
        {
            entity.HasOne(rp => rp.Route)
                .WithMany(r => r.Points)
                .HasForeignKey(rp => rp.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LocationPin>(entity =>
        {
            entity.HasOne(lp => lp.User)
                .WithMany(u => u.LocationPins)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RouteShare>(entity =>
        {
            entity.HasIndex(rs => rs.ShareToken).IsUnique();
            entity.HasOne(rs => rs.Route)
                .WithMany(r => r.Shares)
                .HasForeignKey(rs => rs.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rs => rs.SharedByUser)
                .WithMany(u => u.SharedRoutes)
                .HasForeignKey(rs => rs.SharedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
