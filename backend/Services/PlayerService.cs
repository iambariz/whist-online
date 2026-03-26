using System.Security.Claims;
using WhistOnline.API.DTOs;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;

namespace WhistOnline.API.Services;

public class PlayerService
{
    private readonly PlayerRepository _playerRepository;

    public PlayerService(PlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public Player? FindPlayerByGuid(Guid id)
    {
        return _playerRepository.FindById(id);
    }

    public Player? CreatePlayer(CreatePlayerRequest request)
    {
        var newPlayer = new Player { Name = request.Name };
        _playerRepository.Add(newPlayer);
        _playerRepository.SaveChanges();
        return newPlayer;
    }

    public Player? GetPlayerFromToken(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id == null) return null;

        return FindPlayerByGuid(Guid.Parse(id));
    }
}
