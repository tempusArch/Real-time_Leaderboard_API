using System.Net.NetworkInformation;
using System.Security.Cryptography;
using RealtimeLeaderboardAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace RealtimeLeaderboardAPI.Infrastructure;

public class RealtimeLeaderboardAPIDbContext : DbContext {
    public RealtimeLeaderboardAPIDbContext(DbContextOptions<RealtimeLeaderboardAPIDbContext> options) : base(options) {

    }
    public DbSet<Game> GameTable {get; set;}
    public DbSet<Season> SeasonTable {get; set;}
    public DbSet<Score> ScoreTable {get; set;}
    public DbSet<ArchivedLeaderboard> ArchivedLeaderboardTable {get; set;}

    public DbSet<User> UserTable {get; set;}
    public DbSet<RefreshToken> RefreshTokenTable {get; set;}

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<decimal>()
            .HavePrecision(10, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();


        modelBuilder.Entity<Game>().HasIndex(x => x.Title).IsUnique();


        modelBuilder.Entity<Season>().HasIndex(x => new {x.Name, x.GameId}).IsUnique();
        modelBuilder.Entity<Season>()
            .ToTable(t => t.HasCheckConstraint("CK_SeasonTable_SeasonStatus", "\"SeasonStatus\" IN (0, 1, 2)"));


        modelBuilder.Entity<Score>().HasIndex(x => new {x.SeasonId, x.UserId}).IsUnique();
        modelBuilder.Entity<Score>()
            .ToTable(t => t.HasCheckConstraint("CK_Score_CurrentRankingScore_NonNegative", "\"CurrentRankingScore\" >= 0"));
        modelBuilder.Entity<Score>()
            .ToTable(t => t.HasCheckConstraint("CK_Score_HighestRankingScore_NonNegative", "\"HighestRankingScore\" >= 0"));


        modelBuilder.Entity<Game>()
            .HasMany(x => x.SeasonRisuto)
            .WithOne(z => z.Game)
            .HasForeignKey(z => z.GameId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Season>()
            .HasMany(x => x.ScoreRisuto)
            .WithOne(z => z.Season)
            .HasForeignKey(z => z.SeasonId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(x => x.ScoreRisuto)
            .WithOne(z => z.User)
            .HasForeignKey(z => z.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
       
    
    }
}