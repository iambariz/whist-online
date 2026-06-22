namespace WhistOnline.Tests.Services;

using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;
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

    private LobbyService CreateService(AppDbContext db) =>
        new LobbyService(new GameRepository(db), new PlayerRepository(db));

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

        var result = CreateService(db).FindOpenLobbies();

        Assert.Equal(2, result.Count);
        Assert.All(result, g => Assert.Equal(GameStatus.Waiting, g.Status));
    }

    [Fact]
    public void FindOpenLobbies_ReturnsEmpty_WhenNoneWaiting()
    {
        var db = CreateDb();
        db.Games.Add(new Game { Status = GameStatus.Bidding });
        db.SaveChanges();

        var result = CreateService(db).FindOpenLobbies();

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

        var result = CreateService(db).CreateGameForPlayer(player);

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

        var result = CreateService(db).CreateGameForPlayer(player);

        Assert.Equal(0, result.Players[0].SeatIndex);
    }

    [Fact]
    public void CreateGameForPlayer_SetsStatusToWaiting()
    {
        var db = CreateDb();
        var player = new Player { Name = "Alice" };
        db.Players.Add(player);
        db.SaveChanges();

        var result = CreateService(db).CreateGameForPlayer(player);

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

        var result = CreateService(db).DeleteGame(game.Id);
        Assert.True(result);
    }

    [Fact]
    public void DeleteGame_ReturnsFalse_WhenGameNotFound()
    {
        var db = CreateDb();

        var result = CreateService(db).DeleteGame(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public void DeleteGame_RemovesGameFromDb()
    {
        var db = CreateDb();
        var game = new Game();
        db.Games.Add(game);
        db.SaveChanges();

        CreateService(db).DeleteGame(game.Id);

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

        var result = CreateService(db).JoinLobby(game.Id, player.Id);

        Assert.Equal(JoinLobbyResult.Success, result);
    }

    [Fact]
    public void JoinLobby_ReturnsFalse_WhenPlayerNotFound()
    {
        var db = CreateDb();
        var game = new Game { Status = GameStatus.Waiting };
        db.Games.Add(game);
        db.SaveChanges();

        var result = CreateService(db).JoinLobby(game.Id, Guid.NewGuid());

        Assert.Equal(JoinLobbyResult.LobbyNotFound, result);
    }

    [Fact]
    public void JoinLobby_ReturnsFalse_WhenLobbyNotFound()
    {
        var db = CreateDb();
        var player = new Player { Name = "Bob" };
        db.Players.Add(player);
        db.SaveChanges();

        var result = CreateService(db).JoinLobby(Guid.NewGuid(), player.Id);

        Assert.Equal(JoinLobbyResult.LobbyNotFound, result);
    }

    [Fact]
    public void JoinLobby_ReturnsFalse_WhenPlayerAlreadyInLobby()
    {
        var db = CreateDb();
        var player = new Player { Name = "Bob" };
        var game = new Game { Status = GameStatus.Waiting, Players = new List<Player> { player } };
        db.Games.Add(game);
        db.SaveChanges();

        var result = CreateService(db).JoinLobby(game.Id, player.Id);

        Assert.Equal(JoinLobbyResult.AlreadyInLobby, result);
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

        var result = CreateService(db).JoinLobby(game.Id, player.Id);

        Assert.Equal(JoinLobbyResult.LobbyNotFound, result);
    }

    // LeaveLobby tests

    [Fact]
    public void LeaveLobby_ReturnsNotInLobby_WhenPlayerNotMember()
    {
        var db = CreateDb();
        var game = new Game { Status = GameStatus.Waiting };
        db.Games.Add(game);
        db.SaveChanges();

        var result = CreateService(db).LeaveLobby(game.Id, Guid.NewGuid());

        Assert.Equal(LeaveLobbyResult.NotInLobby, result);
    }

    [Fact]
    public void LeaveLobby_ReturnsLobbyNotFound_WhenGameMissing()
    {
        var db = CreateDb();

        var result = CreateService(db).LeaveLobby(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(LeaveLobbyResult.LobbyNotFound, result);
    }

    [Fact]
    public void LeaveLobby_ReturnsGameInProgress_WhenNotWaiting()
    {
        var db = CreateDb();
        var player = new Player { Name = "Bob" };
        var game = new Game { Status = GameStatus.Playing, Players = new List<Player> { player } };
        db.Games.Add(game);
        db.SaveChanges();

        var result = CreateService(db).LeaveLobby(game.Id, player.Id);

        Assert.Equal(LeaveLobbyResult.GameInProgress, result);
    }

    [Fact]
    public void LeaveLobby_RemovesPlayer_AndDeletesEmptyLobby()
    {
        var db = CreateDb();
        var player = new Player { Name = "Alice" };
        db.Players.Add(player);
        db.SaveChanges();

        var service = CreateService(db);
        var game = service.CreateGameForPlayer(player);

        var result = service.LeaveLobby(game.Id, player.Id);

        Assert.Equal(LeaveLobbyResult.Success, result);
        Assert.Null(db.Games.Find(game.Id));
    }

    [Fact]
    public void LeaveLobby_ReassignsHost_AndRepacksSeats_WhenHostLeaves()
    {
        var db = CreateDb();
        var host = new Player { Name = "Alice" };
        var other = new Player { Name = "Bob" };
        db.Players.AddRange(host, other);
        db.SaveChanges();

        var service = CreateService(db);
        var game = service.CreateGameForPlayer(host);
        service.JoinLobby(game.Id, other.Id);

        var result = service.LeaveLobby(game.Id, host.Id);

        Assert.Equal(LeaveLobbyResult.Success, result);
        var remaining = db.Games.Find(game.Id)!;
        Assert.Single(remaining.Players);
        Assert.Equal(other.Id, remaining.HostPlayerId);
        Assert.Equal(0, db.Players.Find(other.Id)!.SeatIndex);
    }
}
