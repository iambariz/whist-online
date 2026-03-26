namespace WhistOnline.Tests.Services;

using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;
using WhistOnline.API.Services;

public class BidServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private BidService CreateService(AppDbContext db) =>
        new BidService(new GameRepository(db), new BidRepository(db));

    private Game CreateGameInBiddingWithRound(AppDbContext db, int playerCount, int cardsDealt = 5)
    {
        var players = Enumerable.Range(0, playerCount)
            .Select(i => new Player { Name = $"Player{i}", SeatIndex = i })
            .ToList();

        var round = new Round { RoundNumber = 1, CardsDealt = cardsDealt };
        var game = new Game
        {
            Status = GameStatus.Bidding,
            CurrentRound = 1,
            DealerIndex = playerCount - 1, // last player is dealer
            Players = players,
            Rounds = new List<Round> { round }
        };

        db.Games.Add(game);
        db.SaveChanges();
        return game;
    }

    // SubmitBid tests

    [Fact]
    public void SubmitBid_ReturnsNull_WhenGameNotFound()
    {
        var db = CreateDb();
        var service = CreateService(db);

        var result = service.SubmitBid(Guid.NewGuid(), Guid.NewGuid(), 1);

        Assert.Null(result);
    }

    [Fact]
    public void SubmitBid_ReturnsNull_WhenGameNotInBiddingState()
    {
        var db = CreateDb();
        var player = new Player { Name = "Alice", SeatIndex = 0 };
        var game = new Game { Status = GameStatus.Waiting, Players = new List<Player> { player } };
        db.Games.Add(game);
        db.SaveChanges();

        var result = CreateService(db).SubmitBid(game.Id, player.Id, 1);

        Assert.Null(result);
    }

    [Fact]
    public void SubmitBid_ReturnsNull_WhenPlayerNotInGame()
    {
        var db = CreateDb();
        var game = CreateGameInBiddingWithRound(db, 3);

        var result = CreateService(db).SubmitBid(game.Id, Guid.NewGuid(), 1);

        Assert.Null(result);
    }

    [Fact]
    public void SubmitBid_ReturnsNull_WhenNoCurrentRound()
    {
        var db = CreateDb();
        var player = new Player { Name = "Alice", SeatIndex = 0 };
        var game = new Game
        {
            Status = GameStatus.Bidding,
            CurrentRound = 99, // no round with this number
            Players = new List<Player> { player }
        };
        db.Games.Add(game);
        db.SaveChanges();

        var result = CreateService(db).SubmitBid(game.Id, player.Id, 1);

        Assert.Null(result);
    }

    [Fact]
    public void SubmitBid_ReturnsNull_WhenNotPlayersTurn()
    {
        var db = CreateDb();
        var game = CreateGameInBiddingWithRound(db, 3);
        // DealerIndex = 2, so first bidder is seat 0, second is seat 1
        var secondPlayer = game.Players.First(p => p.SeatIndex == 1);

        var result = CreateService(db).SubmitBid(game.Id, secondPlayer.Id, 1);

        Assert.Null(result);
    }

    [Fact]
    public void SubmitBid_ReturnsNull_WhenBidExceedsCardsDealt()
    {
        var db = CreateDb();
        var game = CreateGameInBiddingWithRound(db, 3, cardsDealt: 5);
        var firstPlayer = game.Players.First(p => p.SeatIndex == 0);

        var result = CreateService(db).SubmitBid(game.Id, firstPlayer.Id, 6);

        Assert.Null(result);
    }

    [Fact]
    public void SubmitBid_ReturnsNull_WhenDealerBidMakesTotalEqualCardsDealt()
    {
        var db = CreateDb();
        // 3 players, dealer is seat 2, cards dealt = 3
        var game = CreateGameInBiddingWithRound(db, 3, cardsDealt: 3);
        var round = game.Rounds.First();

        // First two players bid 1 each (total = 2), dealer cannot bid 1 (2+1=3=cardsDealt)
        var p0 = game.Players.First(p => p.SeatIndex == 0);
        var p1 = game.Players.First(p => p.SeatIndex == 1);
        round.Bids.Add(new Bid { PlayerId = p0.Id, Amount = 1, RoundId = round.Id });
        round.Bids.Add(new Bid { PlayerId = p1.Id, Amount = 1, RoundId = round.Id });
        db.SaveChanges();

        var dealer = game.Players.First(p => p.SeatIndex == game.DealerIndex);
        var result = CreateService(db).SubmitBid(game.Id, dealer.Id, 1);

        Assert.Null(result);
    }

    [Fact]
    public void SubmitBid_ReturnsBid_WhenValid()
    {
        var db = CreateDb();
        var game = CreateGameInBiddingWithRound(db, 3);
        var firstPlayer = game.Players.First(p => p.SeatIndex == 0);

        var result = CreateService(db).SubmitBid(game.Id, firstPlayer.Id, 1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Amount);
        Assert.Equal(firstPlayer.Id, result.PlayerId);
    }

    [Fact]
    public void SubmitBid_AdvancesCurrentPlayerIndex()
    {
        var db = CreateDb();
        var game = CreateGameInBiddingWithRound(db, 3);
        var firstPlayer = game.Players.First(p => p.SeatIndex == 0);
        var initialIndex = game.CurrentPlayerIndex;

        CreateService(db).SubmitBid(game.Id, firstPlayer.Id, 1);

        var updatedGame = db.Games.Find(game.Id)!;
        Assert.NotEqual(initialIndex, updatedGame.CurrentPlayerIndex);
    }
}
