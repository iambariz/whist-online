namespace WhistOnline.Tests.Services;

using WhistOnline.API.Models;
using WhistOnline.API.Services;

public class ScoreBoardServiceTests
{
    private readonly ScoreBoardService _service = new();

    private Game CreateGame(List<(string name, int seatIndex, int score)> playerData)
    {
        var players = playerData
            .Select(p => new Player { Id = Guid.NewGuid(), Name = p.name, SeatIndex = p.seatIndex, Score = p.score })
            .ToList();
        return new Game { Players = players };
    }

    [Fact]
    public void GetScoreList_ReturnsSortedByScoreDescending()
    {
        var game = CreateGame([("Alice", 0, 5), ("Bob", 1, 20), ("Carol", 2, 10)]);

        var result = _service.GetScoreList(game);

        Assert.Equal(20, result[0].Score);
        Assert.Equal(10, result[1].Score);
        Assert.Equal(5,  result[2].Score);
    }

    [Fact]
    public void GetScoreList_MapsNameAndSeatIndex()
    {
        var game = CreateGame([("Alice", 0, 10)]);

        var result = _service.GetScoreList(game);

        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(0, result[0].SeatIndex);
        Assert.Equal(10, result[0].Score);
    }

    [Fact]
    public void GetScoreList_HandlesNegativeScores()
    {
        var game = CreateGame([("Alice", 0, -5), ("Bob", 1, 10)]);

        var result = _service.GetScoreList(game);

        Assert.Equal(10, result[0].Score);
        Assert.Equal(-5, result[1].Score);
    }

    [Fact]
    public void GetScoreList_ReturnsAllPlayers()
    {
        var game = CreateGame([("Alice", 0, 10), ("Bob", 1, 20), ("Carol", 2, 5)]);

        var result = _service.GetScoreList(game);

        Assert.Equal(3, result.Count);
    }
}
