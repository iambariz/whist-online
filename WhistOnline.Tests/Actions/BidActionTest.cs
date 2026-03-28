namespace WhistOnline.Tests.Actions;

using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Actions;
using WhistOnline.API.Data;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;

public class BidActionTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private BidAction CreateAction(AppDbContext db, int amount) =>
        new BidAction(new GameRules(), new BidRepository(db), amount);

    private (Game game, Round round) CreateGameInBidding(int playerCount, int cardsDealt = 5)
    {
        var players = Enumerable.Range(0, playerCount)
            .Select(i => new Player { Name = $"Player{i}", SeatIndex = i })
            .ToList();

        var round = new Round { RoundNumber = 1, CardsDealt = cardsDealt };
        var game = new Game
        {
            Status = GameStatus.Bidding,
            CurrentRound = 1,
            CurrentPlayerIndex = 0,
            DealerIndex = playerCount - 1,
            Players = players,
            Rounds = new List<Round> { round }
        };

        return (game, round);
    }

    [Fact]
    public void Execute_ReturnsFalse_WhenGameNotInBiddingState()
    {
        var db = CreateDb();
        var (game, _) = CreateGameInBidding(3);
        game.Status = GameStatus.Waiting;
        var player = game.Players.First(p => p.SeatIndex == 0);

        var result = CreateAction(db, 1).Execute(game, player);

        Assert.False(result);
    }

    [Fact]
    public void Execute_ReturnsFalse_WhenNoCurrentRound()
    {
        var db = CreateDb();
        var (game, _) = CreateGameInBidding(3);
        game.CurrentRound = 99;
        var player = game.Players.First(p => p.SeatIndex == 0);

        var result = CreateAction(db, 1).Execute(game, player);

        Assert.False(result);
    }

    [Fact]
    public void Execute_ReturnsFalse_WhenNotPlayersTurn()
    {
        var db = CreateDb();
        var (game, _) = CreateGameInBidding(3);
        // CurrentPlayerIndex = 0, so seat 1 is not their turn
        var player = game.Players.First(p => p.SeatIndex == 1);

        var result = CreateAction(db, 1).Execute(game, player);

        Assert.False(result);
    }

    [Fact]
    public void Execute_ReturnsFalse_WhenBidExceedsCardsDealt()
    {
        var db = CreateDb();
        var (game, _) = CreateGameInBidding(3, cardsDealt: 5);
        var player = game.Players.First(p => p.SeatIndex == 0);

        var result = CreateAction(db, 6).Execute(game, player);

        Assert.False(result);
    }

    [Fact]
    public void Execute_ReturnsFalse_WhenDealerBidMakesTotalEqualCardsDealt()
    {
        var db = CreateDb();
        var (game, round) = CreateGameInBidding(3, cardsDealt: 3);

        // Simulate first two players already bid 1 each
        var p0 = game.Players.First(p => p.SeatIndex == 0);
        var p1 = game.Players.First(p => p.SeatIndex == 1);
        round.Bids.Add(new Bid { PlayerId = p0.Id, Amount = 1, RoundId = round.Id });
        round.Bids.Add(new Bid { PlayerId = p1.Id, Amount = 1, RoundId = round.Id });

        // It's now the dealer's turn (seat 2)
        game.CurrentPlayerIndex = 2;
        var dealer = game.Players.First(p => p.SeatIndex == game.DealerIndex);

        // 2 + 1 = 3 = cardsDealt — not allowed
        var result = CreateAction(db, 1).Execute(game, dealer);

        Assert.False(result);
    }

    [Fact]
    public void Execute_ReturnsTrue_WhenValid()
    {
        var db = CreateDb();
        var (game, _) = CreateGameInBidding(3);
        var player = game.Players.First(p => p.SeatIndex == 0);

        var action = CreateAction(db, 1);
        var result = action.Execute(game, player);

        Assert.True(result);
        Assert.NotNull(action.Result);
        Assert.Equal(1, action.Result!.Amount);
        Assert.Equal(player.Id, action.Result.PlayerId);
    }

    [Fact]
    public void Execute_AdvancesCurrentPlayerIndex()
    {
        var db = CreateDb();
        var (game, _) = CreateGameInBidding(3);
        var player = game.Players.First(p => p.SeatIndex == 0);

        CreateAction(db, 1).Execute(game, player);

        Assert.Equal(1, game.CurrentPlayerIndex);
    }
}
