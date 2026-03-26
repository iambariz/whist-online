namespace WhistOnline.Tests.Services;

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.DTOs;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;
using WhistOnline.API.Services;


public class PlayerServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private PlayerService CreateService(AppDbContext db) =>
        new PlayerService(new PlayerRepository(db));

    // FindPlayerByGuid tests

    [Fact]
    public void FindPlayerByGuid_ReturnsPlayer_WhenExists()
    {
        var db = CreateDb();
        var player = new Player { Name = "Alice" };
        db.Players.Add(player);
        db.SaveChanges();

        var result = CreateService(db).FindPlayerByGuid(player.Id);

        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public void FindPlayerByGuid_ReturnsNull_WhenNotFound()
    {
        var db = CreateDb();

        var result = CreateService(db).FindPlayerByGuid(Guid.NewGuid());

        Assert.Null(result);
    }

    // CreatePlayer tests

    [Fact]
    public void CreatePlayer_ReturnsPlayerWithCorrectName()
    {
        var db = CreateDb();
        var request = new CreatePlayerRequest { Name = "Bob" };

        var result = CreateService(db).CreatePlayer(request);

        Assert.NotNull(result);
        Assert.Equal("Bob", result.Name);
    }

    [Fact]
    public void CreatePlayer_PersistsPlayerInDb()
    {
        var db = CreateDb();
        var request = new CreatePlayerRequest { Name = "Bob" };
        var service = CreateService(db);

        var created = service.CreatePlayer(request);
        var found = service.FindPlayerByGuid(created!.Id);

        Assert.NotNull(found);
    }

    // GetPlayerFromToken tests

    [Fact]
    public void GetPlayerFromToken_ReturnsPlayer_WhenClaimIsValid()
    {
        var db = CreateDb();
        var player = new Player { Name = "Alice" };
        db.Players.Add(player);
        db.SaveChanges();

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString())
        }));

        var result = CreateService(db).GetPlayerFromToken(claims);

        Assert.NotNull(result);
        Assert.Equal(player.Id, result.Id);
    }

    [Fact]
    public void GetPlayerFromToken_ReturnsNull_WhenClaimIsMissing()
    {
        var db = CreateDb();
        var claims = new ClaimsPrincipal(new ClaimsIdentity());

        var result = CreateService(db).GetPlayerFromToken(claims);

        Assert.Null(result);
    }
}
