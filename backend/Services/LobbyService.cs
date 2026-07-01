using Bogus;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;

namespace WhistOnline.API.Services;

public class LobbyService
{
    private readonly GameRepository _gameRepository;
    private readonly PlayerRepository _playerRepository;

    public LobbyService(GameRepository gameRepository, PlayerRepository playerRepository)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
    }

    public Game? FindLobbyById(Guid id)
    {
        return _gameRepository.FindByIdWithPlayers(id);
    }

    public List<Game> FindOpenLobbies()
    {
        return _gameRepository.FindOpenLobbies();
    }

    //Todo: Rate limiting
    public Game CreateGameForPlayer(Player player, string? name = null)
    {
        if (name == null)
        {
            var faker = new Faker();
            name = faker.Hacker.Adjective() + faker.Hacker.Noun();
        }
        player.SeatIndex = 0;
        var newGame = new Game { Name = name, HostPlayerId = player.Id, Players = new List<Player> { player } };
        _gameRepository.Add(newGame);
        _gameRepository.SaveChanges();
        return newGame;
    }

    //Todo: JWT + User check
    public bool DeleteGame(Guid id)
    {
        var game = _gameRepository.FindById(id);
        if (game == null) return false;

        _gameRepository.Remove(game);
        _gameRepository.SaveChanges();
        return true;
    }

    // Dev-only helper: seats a throwaway bot so a single dev can reach MinPlayers.
    public JoinLobbyResult AddDummyPlayer(Guid lobbyId)
    {
        var lobby = _gameRepository.FindOpenLobbyByIdWithPlayers(lobbyId);
        if (lobby == null) return JoinLobbyResult.LobbyNotFound;
        if (lobby.Players.Count >= lobby.MaxPlayers) return JoinLobbyResult.LobbyFull;

        var faker = new Faker();
        var dummy = new Player
        {
            Name = faker.Name.FirstName() + " (bot)",
            GameId = lobby.Id,
            IsConnected = true,
            SeatIndex = Enumerable.Range(0, lobby.MaxPlayers)
                .First(i => lobby.Players.All(p => p.SeatIndex != i))
        };
        lobby.Players.Add(dummy);

        _gameRepository.SaveChanges();
        return JoinLobbyResult.Success;
    }

    public JoinLobbyResult JoinLobby(Guid id, Guid playerId)
    {
        var player = _playerRepository.FindByIdTracked(playerId);
        if (player == null) return JoinLobbyResult.LobbyNotFound;

        var lobby = _gameRepository.FindOpenLobbyByIdWithPlayers(id);

        if (lobby == null) return JoinLobbyResult.LobbyNotFound;

        if (lobby.Players.Any(p => p.Id == player.Id)) return JoinLobbyResult.AlreadyInLobby;

        if (lobby.Players.Count >= lobby.MaxPlayers) return JoinLobbyResult.LobbyFull;

        // Pick the lowest free seat so indices stay unique even after leaves/rejoins
        player.SeatIndex = Enumerable.Range(0, lobby.MaxPlayers)
            .First(i => lobby.Players.All(p => p.SeatIndex != i));
        lobby.Players.Add(player);

        _gameRepository.SaveChanges();
        return JoinLobbyResult.Success;
    }

    public LeaveLobbyResult LeaveLobby(Guid id, Guid playerId)
    {
        var lobby = _gameRepository.FindByIdWithPlayers(id);
        if (lobby == null) return LeaveLobbyResult.LobbyNotFound;
        if (lobby.Status != GameStatus.Waiting) return LeaveLobbyResult.GameInProgress;

        var player = lobby.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return LeaveLobbyResult.NotInLobby;

        lobby.Players.Remove(player);
        player.GameId = null;
        player.SeatIndex = 0;

        if (lobby.Players.Count == 0)
        {
            _gameRepository.Remove(lobby);
            _gameRepository.SaveChanges();
            return LeaveLobbyResult.Success;
        }

        // Keep seat indices contiguous so StartGame's seat-based dealing stays valid
        var reseated = lobby.Players.OrderBy(p => p.SeatIndex).ToList();
        for (int i = 0; i < reseated.Count; i++) reseated[i].SeatIndex = i;

        // If the host left, hand off to the new first seat
        if (lobby.HostPlayerId == playerId) lobby.HostPlayerId = reseated[0].Id;

        _gameRepository.SaveChanges();
        return LeaveLobbyResult.Success;
    }
}
