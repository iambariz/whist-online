namespace WhistOnline.Tests.Services;

using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.Models;
using WhistOnline.API.Services;

public class LobbyServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // FindOpenLobbies tests

    [Fact]
    public void FindOpenLobbies_ReturnsOnlyWaitingGames()
    {
        var db = CreateDb();
        db.Games.AddRange(
            new Game { Status = GameStatus.Waiting },
            new Game { Status = GameStatus.Waiting },
            new Game { Status = GameStatus.Bidding }
        );
        db.SaveChanges();

        var result = new LobbyService(db).FindOpenLobbies();

        Assert.Equal(2, result.Count);
        Assert.All(result, g => Assert.Equal(GameStatus.Waiting, g.Status));
    }

    [Fact]
    public void FindOpenLobbies_ReturnsEmpty_WhenNoneWaiting()
    {
        var db = CreateDb();
        db.Games.Add(new Game { Status = GameStatus.Bidding });
        db.SaveChanges();

        var result = new LobbyService(db).FindOpenLobbies();

        Assert.Empty(result);
    }

    // CreateGameForPlayer tests

    [Fact]
    public void CreateGameForPlayer_ReturnsGameWithPlayer()
    {
        var db = CreateDb();
        var player = new Player { Name = "Alice" };
        db.Players.Add(player);
        db.SaveChanges();

        var result = new LobbyService(db).CreateGameForPlayer(player);

        Assert.NotNull(result);
        Assert.Single(result.Players);
        Assert.Equal(player.Id, result.Players[0].Id);
    }

    [Fact]
    public void CreateGameForPlayer_SetsSeatIndexToZero()
    {
        var db = CreateDb();
        var player = new Player { Name = "Alice", SeatIndex = 99 };
        db.Players.Add(player);
        db.SaveChanges();

        var result = new LobbyService(db).CreateGameForPlayer(player);

        Assert.Equal(0, result.Players[0].SeatIndex);
    }

    [Fact]
    public void CreateGameForPlayer_SetsStatusToWaiting()
    {
        var db = CreateDb();
        var player = new Player { Name = "Alice" };
        db.Players.Add(player);
        db.SaveChanges();

        var result = new LobbyService(db).CreateGameForPlayer(player);

        Assert.Equal(GameStatus.Waiting, result.Status);
    }

    // DeleteGame tests

    [Fact]
    public void DeleteGame_ReturnsTrue_WhenGameExists()
    {
        var db = CreateDb();
        var game = new Game();
        db.Games.Add(game);
        db.SaveChanges();

        var result = new LobbyService(db).DeleteGame(game.Id);

        Assert.True(result);
    }

    [Fact]
    public void DeleteGame_ReturnsFalse_WhenGameNotFound()
    {
        var db = CreateDb();

        var result = new LobbyService(db).DeleteGame(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public void DeleteGame_RemovesGameFromDb()
    {
        var db = CreateDb();
        var game = new Game();
        db.Games.Add(game);
        db.SaveChanges();

        new LobbyService(db).DeleteGame(game.Id);

        Assert.Null(db.Games.Find(game.Id));
    }

    // JoinLobby tests

    [Fact]
    public void JoinLobby_ReturnsTrue_WhenValid()
    {
        var db = CreateDb();
        var player = new Player { Name = "Bob" };
        var game = new Game { Status = GameStatus.Waiting };
        db.Players.Add(player);
        db.Games.Add(game);
        db.SaveChanges();

        var result = new LobbyService(db).JoinLobby(game.Id, player.Id);

        Assert.True(result);
    }

    [Fact]
    public void JoinLobby_ReturnsFalse_WhenPlayerNotFound()
    {
        var db = CreateDb();
        var game = new Game { Status = GameStatus.Waiting };
        db.Games.Add(game);
        db.SaveChanges();

        var result = new LobbyService(db).JoinLobby(game.Id, Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public void JoinLobby_ReturnsFalse_WhenLobbyNotFound()
    {
        var db = CreateDb();
        var player = new Player { Name = "Bob" };
        db.Players.Add(player);
        db.SaveChanges();

        var result = new LobbyService(db).JoinLobby(Guid.NewGuid(), player.Id);

        Assert.False(result);
    }

    [Fact]
    public void JoinLobby_ReturnsFalse_WhenPlayerAlreadyInLobby()
    {
        var db = CreateDb();
        var player = new Player { Name = "Bob" };
        var game = new Game { Status = GameStatus.Waiting, Players = new List<Player> { player } };
        db.Games.Add(game);
        db.SaveChanges();

        var result = new LobbyService(db).JoinLobby(game.Id, player.Id);

        Assert.False(result);
    }

    [Fact]
    public void JoinLobby_ReturnsFalse_WhenGameNotWaiting()
    {
        var db = CreateDb();
        var player = new Player { Name = "Bob" };
        var game = new Game { Status = GameStatus.Bidding };
        db.Players.Add(player);
        db.Games.Add(game);
        db.SaveChanges();

        var result = new LobbyService(db).JoinLobby(game.Id, player.Id);

        Assert.False(result);
    }
}
