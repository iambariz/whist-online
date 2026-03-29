namespace WhistOnline.Tests.Actions;

using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Actions;
using WhistOnline.API.Data;
using WhistOnline.API.DTOs;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;
using WhistOnline.API.Services;

public class PlayCardActionTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private GameService CreateGameService(AppDbContext db) =>
        new GameService(new GameRepository(db), new DeckService());

    private PlayCardAction CreateAction(AppDbContext db, Suit suit, Rank rank) =>
        new PlayCardAction(
            new GameRules(),
            new PlayCardDto { Suit = suit, Rank = rank },
            new TrickService(),
            new ScoringService(),
            CreateGameService(db)
        );

    private (Game game, Player player) CreateGameInPlaying(AppDbContext db, int playerCount = 2)
    {
        var players = Enumerable.Range(0, playerCount)
            .Select(i => new Player
            {
                Id = Guid.NewGuid(),
                SeatIndex = i,
                Hand = new List<Card>
                {
                    new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                    new Card { Suit = Suit.Clubs, Rank = Rank.King }
                }
            })
            .ToList();

        var trick = new Trick { LeadSuit = null, CardsPlayed = [] };
        var round = new Round { RoundNumber = 1, CardsDealt = 2, Tricks = [trick] };
        var game = new Game
        {
            Status = GameStatus.Playing,
            CurrentRound = 1,
            TotalRounds = 3,
            TrumpSuit = Suit.Spades,
            CurrentPlayerIndex = 0,
            Players = players,
            Rounds = [round]
        };

        return (game, players[0]);
    }

    [Fact]
    public void Execute_ReturnsFalse_WhenNotPlayersTurn()
    {
        var db = CreateDb();
        var (game, _) = CreateGameInPlaying(db);
        var otherPlayer = game.Players[1];

        var result = CreateAction(db, Suit.Hearts, Rank.Ace).Execute(game, otherPlayer);

        Assert.False(result);
    }

    [Fact]
    public void Execute_ReturnsFalse_WhenPlayerDoesNotHaveCard()
    {
        var db = CreateDb();
        var (game, player) = CreateGameInPlaying(db);

        var result = CreateAction(db, Suit.Diamonds, Rank.Two).Execute(game, player);

        Assert.False(result);
    }

    [Fact]
    public void Execute_ReturnsFalse_WhenPlayerMustFollowSuitButPlaysAnother()
    {
        var db = CreateDb();
        var (game, player) = CreateGameInPlaying(db);
        game.Rounds.Last().Tricks.Last().LeadSuit = Suit.Hearts;

        // Player has Hearts but tries to play Clubs
        var result = CreateAction(db, Suit.Clubs, Rank.King).Execute(game, player);

        Assert.False(result);
    }

    [Fact]
    public void Execute_RemovesCardFromHand()
    {
        var db = CreateDb();
        var (game, player) = CreateGameInPlaying(db);

        CreateAction(db, Suit.Hearts, Rank.Ace).Execute(game, player);

        Assert.DoesNotContain(player.Hand, c => c.Suit == Suit.Hearts && c.Rank == Rank.Ace);
    }

    [Fact]
    public void Execute_SetsLeadSuit_OnFirstCardPlayed()
    {
        var db = CreateDb();
        var (game, player) = CreateGameInPlaying(db);
        var trick = game.Rounds.Last().Tricks.Last();

        CreateAction(db, Suit.Hearts, Rank.Ace).Execute(game, player);

        Assert.Equal(Suit.Hearts, trick.LeadSuit);
    }

    [Fact]
    public void Execute_EvaluatesTrick_WhenAllPlayersHavePlayed()
    {
        var db = CreateDb();
        var (game, p0) = CreateGameInPlaying(db, playerCount: 2);
        var trick = game.Rounds.Last().Tricks.Last();
        trick.LeadSuit = Suit.Hearts;

        // Simulate p1 already having played
        var p1 = game.Players[1];
        trick.CardsPlayed.Add(new CardPlay
        {
            PlayerId = p1.Id,
            Card = new Card { Suit = Suit.Hearts, Rank = Rank.Two }
        });
        p1.Hand.RemoveAll(c => c.Suit == Suit.Hearts && c.Rank == Rank.Two);

        CreateAction(db, Suit.Hearts, Rank.Ace).Execute(game, p0);

        Assert.NotNull(trick.WinnerPlayerId);
    }

    [Fact]
    public void Execute_DoesNotEvaluateTrick_WhenNotAllPlayersHavePlayed()
    {
        var db = CreateDb();
        var (game, player) = CreateGameInPlaying(db, playerCount: 2);
        var trick = game.Rounds.Last().Tricks.Last();

        CreateAction(db, Suit.Hearts, Rank.Ace).Execute(game, player);

        Assert.Null(trick.WinnerPlayerId);
    }
}
