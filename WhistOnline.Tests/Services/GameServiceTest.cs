namespace WhistOnline.Tests.Services;

using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;
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
        new GameService(new GameRepository(db), new DeckService());

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

    // StartGame tests

    [Fact]
    public void StartGame_ReturnsNull_WhenGameNotFound()
    {
        var db = CreateDb();
        var service = CreateService(db);

        var result = service.StartGame(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void StartGame_ReturnsNull_WhenPlayerNotInGame()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);

        var result = service.StartGame(game.Id, Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void StartGame_ReturnsNull_WhenTooFewPlayers()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 2);
        var service = CreateService(db);

        var result = service.StartGame(game.Id, game.Players[0].Id);

        Assert.Null(result);
    }

    [Fact]
    public void StartGame_ReturnsNull_WhenTooManyPlayers()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 8);
        var service = CreateService(db);

        var result = service.StartGame(game.Id, game.Players[0].Id);

        Assert.Null(result);
    }

    [Fact]
    public void StartGame_SetsStatusToBidding_WhenValid()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);

        var result = service.StartGame(game.Id, game.Players[0].Id);

        Assert.NotNull(result);
        Assert.Equal(GameStatus.Bidding, result.Status);
    }

    [Fact]
    public void StartGame_DealsEqualHandsToAllPlayers_WhenValid()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);

        var result = service.StartGame(game.Id, game.Players[0].Id);

        Assert.NotNull(result);
        Assert.All(result.Players, p => Assert.Equal(13, p.Hand.Count)); // 52 / 4 = 13
    }

    [Fact]
    public void StartGame_TrimsAndDealsEqualHands_ForThreePlayers()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 3);
        var service = CreateService(db);

        var result = service.StartGame(game.Id, game.Players[0].Id);

        Assert.NotNull(result);
        Assert.All(result.Players, p => Assert.Equal(16, p.Hand.Count)); // 48 / 3 = 16
    }

    // GetGameState tests

    [Fact]
    public void GetGameState_ReturnsNull_WhenGameNotFound()
    {
        var db = CreateDb();
        var service = CreateService(db);

        var result = service.GetGameState(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void GetGameState_ReturnsNull_WhenPlayerNotInGame()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);

        var result = service.GetGameState(game.Id, Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void GetGameState_ReturnsState_WhenValid()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);
        var playerId = game.Players[0].Id;

        var result = service.GetGameState(game.Id, playerId);

        Assert.NotNull(result);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(4, result.Players!.Count);
    }

    [Fact]
    public void GetGameState_OnlyReturnsMyHand_ForRequestingPlayer()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);
        var playerId = game.Players[0].Id;

        service.StartGame(game.Id, playerId);
        var result = service.GetGameState(game.Id, playerId);

        Assert.NotNull(result);
        Assert.NotNull(result.MyHand);
        Assert.Equal(13, result.MyHand!.Count);
    }

    [Fact]
    public void GetGameState_HidesOtherPlayersHands()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);
        var playerId = game.Players[0].Id;

        service.StartGame(game.Id, playerId);
        var result = service.GetGameState(game.Id, playerId);

        Assert.NotNull(result);
        Assert.All(result.Players!, p => Assert.Equal(13, p.CardCount));
    }

    // AdvanceRound tests

    [Fact]
    public void AdvanceRound_SetsStatusToFinished_WhenLastRound()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = new Game { CurrentRound = 3, TotalRounds = 3 };

        service.AdvanceRound(game);

        Assert.Equal(GameStatus.Finished, game.Status);
    }

    [Fact]
    public void AdvanceRound_IncrementsCurrentRound_WhenNotLastRound()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = new Game { CurrentRound = 1, TotalRounds = 3, TrumpSuit = Suit.Clubs };

        service.AdvanceRound(game);

        Assert.Equal(2, game.CurrentRound);
    }

    [Fact]
    public void AdvanceRound_RotatesTrumpSuit_WhenNotLastRound()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = new Game { CurrentRound = 1, TotalRounds = 3, TrumpSuit = Suit.Clubs };

        service.AdvanceRound(game);

        Assert.Equal(Suit.Diamonds, game.TrumpSuit);
    }

    // RotateTrumpSuit tests

    [Theory]
    [InlineData(Suit.Clubs, Suit.Diamonds)]
    [InlineData(Suit.Diamonds, Suit.Hearts)]
    [InlineData(Suit.Hearts, Suit.Spades)]
    public void AdvanceRound_CyclesThroughSuits(Suit current, Suit expected)
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = new Game { CurrentRound = 1, TotalRounds = 10, TrumpSuit = current };

        service.AdvanceRound(game);

        Assert.Equal(expected, game.TrumpSuit);
    }

    [Fact]
    public void AdvanceRound_SetsNullTrump_WhenLastSuit()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = new Game { CurrentRound = 1, TotalRounds = 10, TrumpSuit = Suit.Spades };

        service.AdvanceRound(game);

        Assert.Null(game.TrumpSuit);
    }

    [Fact]
    public void AdvanceRound_WrapsBackToClubs_WhenNoTrump()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = new Game { CurrentRound = 1, TotalRounds = 10, TrumpSuit = null };

        service.AdvanceRound(game);

        Assert.Equal(Suit.Clubs, game.TrumpSuit);
    }
}
