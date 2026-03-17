using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WhistOnline.API.Models;

namespace WhistOnline.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<Trick> Tricks => Set<Trick>();
    public DbSet<CardPlay> CardPlays => Set<CardPlay>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .Property(p => p.Hand)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<Card>>(v, (JsonSerializerOptions)null!) ?? new List<Card>()
            );

        modelBuilder.Entity<CardPlay>()
            .Property(cp => cp.Card)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<Card>(v, (JsonSerializerOptions)null!)!
            );
    }
}