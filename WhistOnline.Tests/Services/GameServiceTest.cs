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
    public void StartGame_DealsOneCardPerPlayer_ForRoundOne()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);

        var result = service.StartGame(game.Id, game.Players[0].Id);

        Assert.NotNull(result);
        Assert.All(result.Players, p => Assert.Equal(1, p.Hand.Count));
    }

    [Fact]
    public void StartGame_SetsTotalRounds_BasedOnPlayerCount()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);

        var result = service.StartGame(game.Id, game.Players[0].Id);

        Assert.NotNull(result);
        Assert.Equal(13, result.TotalRounds); // 52 / 4 = 13
    }

    [Fact]
    public void StartGame_CreatesFirstRound()
    {
        var db = CreateDb();
        var game = CreateGameWithPlayers(db, 4);
        var service = CreateService(db);

        var result = service.StartGame(game.Id, game.Players[0].Id);

        Assert.NotNull(result);
        Assert.Single(result.Rounds);
        Assert.Equal(1, result.Rounds[0].RoundNumber);
        Assert.Equal(1, result.Rounds[0].CardsDealt);
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
        Assert.Equal(1, result.MyHand!.Count);
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
        Assert.All(result.Players!, p => Assert.Equal(1, p.CardCount));
    }

    // AdvanceRound tests

    private Game CreateGameForAdvance(int currentRound, int totalRounds, Suit? trumpSuit, int playerCount = 4)
    {
        var players = Enumerable.Range(0, playerCount)
            .Select(i => new Player { Id = Guid.NewGuid(), SeatIndex = i })
            .ToList();
        return new Game
        {
            CurrentRound = currentRound,
            TotalRounds = totalRounds,
            TrumpSuit = trumpSuit,
            DealerIndex = 0,
            Players = players
        };
    }

    [Fact]
    public void AdvanceRound_SetsStatusToFinished_WhenLastRound()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = CreateGameForAdvance(3, 3, Suit.Clubs);

        service.AdvanceRound(game);

        Assert.Equal(GameStatus.Finished, game.Status);
    }

    [Fact]
    public void AdvanceRound_IncrementsCurrentRound_WhenNotLastRound()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = CreateGameForAdvance(1, 13, Suit.Clubs);

        service.AdvanceRound(game);

        Assert.Equal(2, game.CurrentRound);
    }

    [Fact]
    public void AdvanceRound_RotatesTrumpSuit_WhenNotLastRound()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = CreateGameForAdvance(1, 13, Suit.Clubs);

        service.AdvanceRound(game);

        Assert.Equal(Suit.Diamonds, game.TrumpSuit);
    }

    [Fact]
    public void AdvanceRound_CreatesNewRound_WhenNotLastRound()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = CreateGameForAdvance(1, 13, Suit.Clubs);

        service.AdvanceRound(game);

        Assert.Single(game.Rounds);
        Assert.Equal(2, game.Rounds[0].RoundNumber);
        Assert.Equal(2, game.Rounds[0].CardsDealt);
    }

    [Fact]
    public void AdvanceRound_DealsNewHands_WhenNotLastRound()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = CreateGameForAdvance(1, 13, Suit.Clubs);

        service.AdvanceRound(game);

        Assert.All(game.Players, p => Assert.Equal(2, p.Hand.Count));
    }

    [Fact]
    public void AdvanceRound_RotatesDealer_WhenNotLastRound()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = CreateGameForAdvance(1, 13, Suit.Clubs);

        service.AdvanceRound(game);

        Assert.Equal(1, game.DealerIndex);
        Assert.Equal(2, game.CurrentPlayerIndex);
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
        var game = CreateGameForAdvance(1, 13, current);

        service.AdvanceRound(game);

        Assert.Equal(expected, game.TrumpSuit);
    }

    [Fact]
    public void AdvanceRound_SetsNullTrump_WhenLastSuit()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = CreateGameForAdvance(1, 13, Suit.Spades);

        service.AdvanceRound(game);

        Assert.Null(game.TrumpSuit);
    }

    [Fact]
    public void AdvanceRound_WrapsBackToClubs_WhenNoTrump()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var game = CreateGameForAdvance(1, 13, null);

        service.AdvanceRound(game);

        Assert.Equal(Suit.Clubs, game.TrumpSuit);
    }
}
