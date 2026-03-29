namespace WhistOnline.Tests.Services;

using WhistOnline.API.Models;
using WhistOnline.API.Services;

public class TrickServiceTests
{
    private readonly TrickService _trickService = new();

    private Player CreatePlayer(int seatIndex) =>
        new Player { Id = Guid.NewGuid(), SeatIndex = seatIndex };

    private CardPlay Play(Player player, Suit suit, Rank rank) =>
        new CardPlay { PlayerId = player.Id, Card = new Card { Suit = suit, Rank = rank } };

    private Game CreateGame(Suit? trumpSuit, List<Player> players) =>
        new Game { TrumpSuit = trumpSuit, Players = players };

    private Trick CreateTrick(Suit leadSuit, List<CardPlay> cards) =>
        new Trick { LeadSuit = leadSuit, CardsPlayed = cards };

    [Fact]
    public void EvaluateTrick_TrumpCardBeatsLeadSuit()
    {
        var p0 = CreatePlayer(0);
        var p1 = CreatePlayer(1);
        var game = CreateGame(Suit.Spades, [p0, p1]);
        var trick = CreateTrick(Suit.Hearts, [
            Play(p0, Suit.Hearts, Rank.Ace),
            Play(p1, Suit.Spades, Rank.Two)
        ]);

        _trickService.EvaluateTrick(game, trick);

        Assert.Equal(p1.Id, trick.WinnerPlayerId);
    }

    [Fact]
    public void EvaluateTrick_HighestTrumpWins_WhenMultipleTrumpsPlayed()
    {
        var p0 = CreatePlayer(0);
        var p1 = CreatePlayer(1);
        var p2 = CreatePlayer(2);
        var game = CreateGame(Suit.Spades, [p0, p1, p2]);
        var trick = CreateTrick(Suit.Hearts, [
            Play(p0, Suit.Hearts, Rank.Ace),
            Play(p1, Suit.Spades, Rank.King),
            Play(p2, Suit.Spades, Rank.Two)
        ]);

        _trickService.EvaluateTrick(game, trick);

        Assert.Equal(p1.Id, trick.WinnerPlayerId);
    }

    [Fact]
    public void EvaluateTrick_HighestLeadSuitWins_WhenNoTrumpPlayed()
    {
        var p0 = CreatePlayer(0);
        var p1 = CreatePlayer(1);
        var p2 = CreatePlayer(2);
        var game = CreateGame(Suit.Spades, [p0, p1, p2]);
        var trick = CreateTrick(Suit.Hearts, [
            Play(p0, Suit.Hearts, Rank.Ten),
            Play(p1, Suit.Hearts, Rank.Ace),
            Play(p2, Suit.Diamonds, Rank.Ace)
        ]);

        _trickService.EvaluateTrick(game, trick);

        Assert.Equal(p1.Id, trick.WinnerPlayerId);
    }

    [Fact]
    public void EvaluateTrick_SetsCurrentPlayerIndex_ToWinnerSeat()
    {
        var p0 = CreatePlayer(0);
        var p1 = CreatePlayer(1);
        var game = CreateGame(Suit.Spades, [p0, p1]);
        var trick = CreateTrick(Suit.Hearts, [
            Play(p0, Suit.Hearts, Rank.Ace),
            Play(p1, Suit.Spades, Rank.Two)
        ]);

        _trickService.EvaluateTrick(game, trick);

        Assert.Equal(p1.SeatIndex, game.CurrentPlayerIndex);
    }

    [Fact]
    public void EvaluateTrick_DoesNotSetWinner_WhenNoTrumpAndNoLeadSuitPlayed()
    {
        var p0 = CreatePlayer(0);
        var p1 = CreatePlayer(1);
        var game = CreateGame(Suit.Spades, [p0, p1]);
        game.CurrentPlayerIndex = 0;
        var trick = CreateTrick(Suit.Hearts, [
            Play(p0, Suit.Clubs, Rank.Ace),
            Play(p1, Suit.Diamonds, Rank.Ace)
        ]);

        _trickService.EvaluateTrick(game, trick);

        Assert.Null(trick.WinnerPlayerId);
        Assert.Equal(0, game.CurrentPlayerIndex);
    }
}
