using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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
    public async Task SubscribeToGame(Guid gameId)
    {
        var player = _playerService.GetPlayerFromToken(Context.User!);
        if (player == null) throw new HubException("Could not identify player");

        if (!_gameService.PlayerBelongsToGame(gameId, player.Id)) throw new HubException("Not a member of this game");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(gameId));
    }

    public static string GroupName(Guid gameId) => $"game-{gameId}";
}
