using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Models;
using WhistOnline.API.Services;

namespace WhistOnline.API.Hubs;

public class GameHub : Hub
{
    private readonly GameService _gameService;
    private readonly PlayerService _playerService;

    public GameHub(GameService gameService, PlayerService playerService)
    {
        _gameService = gameService;
        _playerService = playerService;
    }

    [Authorize]
    public async Task JoinGame(Guid gameId)
    {
    }
}