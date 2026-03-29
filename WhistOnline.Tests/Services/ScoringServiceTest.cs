namespace WhistOnline.Tests.Services;

using WhistOnline.API.Models;
using WhistOnline.API.Services;

public class ScoringServiceTests
{
    private readonly ScoringService _scoringService = new();

    private (Game game, Round round) CreateGame(int playerCount = 3)
    {
        var players = Enumerable.Range(0, playerCount)
            .Select(i => new Player { Id = Guid.NewGuid(), Name = $"Player{i}", SeatIndex = i })
            .ToList();

        var round = new Round { RoundNumber = 1, CardsDealt = 5 };
        var game = new Game
        {
            Status = GameStatus.Playing,
            Players = players,
            Rounds = new List<Round> { round }
        };

        return (game, round);
    }

    private Trick TrickWonBy(Guid winnerId) =>
        new Trick { WinnerPlayerId = winnerId };

    [Fact]
    public void EvaluateRound_AddsCorrectScore_WhenPlayerHitsBid()
    {
        var (game, round) = CreateGame();
        var player = game.Players[0];

        round.Bids.Add(new Bid { PlayerId = player.Id, Amount = 2 });
        round.Tricks.Add(TrickWonBy(player.Id));
        round.Tricks.Add(TrickWonBy(player.Id));

        _scoringService.EvaluateRound(game);

        Assert.Equal(12, player.Score);
    }

    [Fact]
    public void EvaluateRound_SubtractsDifference_WhenPlayerWinsMoreThanBid()
    {
        var (game, round) = CreateGame();
        var player = game.Players[0];

        round.Bids.Add(new Bid { PlayerId = player.Id, Amount = 1 });
        round.Tricks.Add(TrickWonBy(player.Id));
        round.Tricks.Add(TrickWonBy(player.Id));

        _scoringService.EvaluateRound(game);

        Assert.Equal(-1, player.Score);
    }

    [Fact]
    public void EvaluateRound_SubtractsDifference_WhenPlayerWinsLessThanBid()
    {
        var (game, round) = CreateGame();
        var player = game.Players[0];

        round.Bids.Add(new Bid { PlayerId = player.Id, Amount = 3 });
        round.Tricks.Add(TrickWonBy(player.Id));

        _scoringService.EvaluateRound(game);

        Assert.Equal(-2, player.Score);
    }

    [Fact]
    public void EvaluateRound_AddsCorrectScore_WhenPlayerBidsZeroAndWinsNothing()
    {
        var (game, round) = CreateGame();
        var player = game.Players[0];

        round.Bids.Add(new Bid { PlayerId = player.Id, Amount = 0 });

        _scoringService.EvaluateRound(game);

        Assert.Equal(10, player.Score);
    }

    [Fact]
    public void EvaluateRound_ScoresAllPlayers()
    {
        var (game, round) = CreateGame(playerCount: 3);
        var p0 = game.Players[0];
        var p1 = game.Players[1];
        var p2 = game.Players[2];

        round.Bids.Add(new Bid { PlayerId = p0.Id, Amount = 1 });
        round.Bids.Add(new Bid { PlayerId = p1.Id, Amount = 2 });
        round.Bids.Add(new Bid { PlayerId = p2.Id, Amount = 0 });

        round.Tricks.Add(TrickWonBy(p0.Id));
        round.Tricks.Add(TrickWonBy(p1.Id));
        round.Tricks.Add(TrickWonBy(p1.Id));

        _scoringService.EvaluateRound(game);

        Assert.Equal(11, p0.Score); // hit bid of 1
        Assert.Equal(12, p1.Score); // hit bid of 2
        Assert.Equal(10, p2.Score); // hit bid of 0
    }
}
