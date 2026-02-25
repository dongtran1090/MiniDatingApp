using Microsoft.EntityFrameworkCore;
using MiniDatingApp.Models;

namespace MiniDatingApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<DateSuggestion> DateSuggestions => Set<DateSuggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Profile: unique email
        modelBuilder.Entity<Profile>()
            .HasIndex(p => p.Email)
            .IsUnique();

        // Like: unique (LikerId, LikedId)
        modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.LikerId, l.LikedId })
            .IsUnique();

        // Match: unique (UserLowId, UserHighId)
        modelBuilder.Entity<Match>()
            .HasIndex(m => new { m.UserLowId, m.UserHighId })
            .IsUnique();

        // Like has 2 FK to Profile -> NoAction to avoid multiple cascade paths (SQL Server)
        modelBuilder.Entity<Like>()
            .HasOne(l => l.Liker)
            .WithMany()
            .HasForeignKey(l => l.LikerId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Liked)
            .WithMany()
            .HasForeignKey(l => l.LikedId)
            .OnDelete(DeleteBehavior.NoAction);

        // Match has 2 FK to Profile -> NoAction
        modelBuilder.Entity<Match>()
            .HasOne(m => m.UserLow)
            .WithMany()
            .HasForeignKey(m => m.UserLowId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.UserHigh)
            .WithMany()
            .HasForeignKey(m => m.UserHighId)
            .OnDelete(DeleteBehavior.NoAction);

        // AvailabilitySlot -> Match (cascade ok) + -> Profile (no cascade)
        modelBuilder.Entity<AvailabilitySlot>()
            .HasOne(s => s.Match)
            .WithMany()
            .HasForeignKey(s => s.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AvailabilitySlot>()
            .HasOne(s => s.Profile)
            .WithMany()
            .HasForeignKey(s => s.ProfileId)
            .OnDelete(DeleteBehavior.NoAction);

        // DateSuggestion: 1-1 with Match (cascade ok)
        modelBuilder.Entity<Match>()
         .HasOne(m => m.Suggestion)
         .WithOne(s => s.Match!)
         .HasForeignKey<DateSuggestion>(s => s.MatchId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}