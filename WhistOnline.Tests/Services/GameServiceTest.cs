namespace WhistOnline.Tests.Services;

using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.Models;
using WhistOnline.API.Services;

public class GameServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private GameService CreateService(AppDbContext db) =>
        new GameService(db, new DeckService());

    private Game CreateGameWithPlayers(AppDbContext db, int playerCount)
    {
        var players = Enumerable.Range(0, playerCount)
            .Select(i => new Player { Name = $"Player{i}", SeatIndex = i })
            .ToList();

        var game = new Game { Players = players };
        db.Games.Add(game);
        db.SaveChanges();
        return game;
    }

    [Fact]
    public void StartGame_ReturnsNull_WhenGameNotFound()
    {
        var db = CreateDb();
        var service = CreateService(db);

        var result = service.StartGame(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void StartGame_ReturnsNull_WhenTooFewPlayers()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 2);
        var service = CreateService(db);

        var result = service.StartGame(game.Id);

        Assert.Null(result);
    }

    [Fact]
    public void StartGame_ReturnsNull_WhenTooManyPlayers()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 8);
        var service = CreateService(db);

        var result = service.StartGame(game.Id);

        Assert.Null(result);
    }

    [Fact]
    public void StartGame_SetsStatusToBidding_WhenValid()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);

        var result = service.StartGame(game.Id);

        Assert.NotNull(result);
        Assert.Equal(GameStatus.Bidding, result.Status);
    }

    [Fact]
    public void StartGame_DealsEqualHandsToAllPlayers_WhenValid()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);

        var result = service.StartGame(game.Id);

        Assert.NotNull(result);
        Assert.All(result.Players, p => Assert.Equal(13, p.Hand.Count)); // 52 / 4 = 13
    }

    [Fact]
    public void StartGame_TrimsAndDealsEqualHands_ForThreePlayers()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 3);
        var service = CreateService(db);

        var result = service.StartGame(game.Id);

        Assert.NotNull(result);
        Assert.All(result.Players, p => Assert.Equal(16, p.Hand.Count)); // 48 / 3 = 16
    }
}
