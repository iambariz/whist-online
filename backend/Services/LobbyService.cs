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
        return _gameRepository.FindById(id);
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
        var newGame = new Game { Name = name, Players = new List<Player> { player } };
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

    public JoinLobbyResult JoinLobby(Guid id, Guid playerId)
    {
        var player = _playerRepository.FindByIdTracked(playerId);
        if (player == null) return JoinLobbyResult.LobbyNotFound;

        var lobby = _gameRepository.FindOpenLobbyByIdWithPlayers(id);

        if (lobby == null) return JoinLobbyResult.LobbyNotFound;

        if (lobby.Players.Any(p => p.Id == player.Id)) return JoinLobbyResult.AlreadyInLobby;

        if (lobby.Players.Count >= lobby.MaxPlayers) return JoinLobbyResult.LobbyFull;

        lobby.Players.Add(player);

        _gameRepository.SaveChanges();
        return JoinLobbyResult.Success;
    }
}
