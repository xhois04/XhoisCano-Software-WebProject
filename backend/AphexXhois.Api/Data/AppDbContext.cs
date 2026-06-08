using AphexXhois.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AphexXhois.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Friend> Friends => Set<Friend>();
    public DbSet<SongRating> SongRatings => Set<SongRating>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Friend>(e =>
        {
            e.HasIndex(f => new { f.UserId, f.FriendId }).IsUnique();
            e.HasOne(f => f.User)
                .WithMany(u => u.FriendsAdded)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(f => f.FriendUser)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SongRating>(e =>
        {
            e.HasIndex(r => new { r.UserId, r.SpotifyTrackId }).IsUnique();
            e.HasOne(r => r.User)
                .WithMany(u => u.SongRatings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(e =>
        {
            e.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}