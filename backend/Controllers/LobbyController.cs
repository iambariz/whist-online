using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.DTOs;
using WhistOnline.API.Models;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/lobbies")]
public class LobbyController : BaseController
{
    private readonly LobbyService _lobbyService;
    private readonly IWebHostEnvironment _env;

    public LobbyController(LobbyService lobbyService, PlayerService playerService, IWebHostEnvironment env)
        : base(playerService)
    {
        _lobbyService = lobbyService;
        _env = env;
    }

    [HttpGet]
    public IActionResult GetLobbies()
    {
        var lobbies = _lobbyService.FindOpenLobbies();
        return Ok(lobbies);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetLobby(Guid id)
    {
        var lobby = _lobbyService.FindLobbyById(id);
        if (lobby == null) return ApiError(404, "Lobby not found");
        return Ok(lobby);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Create([FromBody] CreateLobbyRequest request)
    {
        var player = GetCurrentPlayer();
        if (player == null) return ApiError(400, "Could not identify player");

        var createdLobby = _lobbyService.CreateGameForPlayer(player, request.Name);
        return Ok(createdLobby);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteLobby(Guid id)
    {
        if (!_lobbyService.DeleteGame(id)) return ApiError(404, "Lobby not found");
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/join")]
    public IActionResult JoinLobby(Guid id)
    {
        var player = GetCurrentPlayer();
        if (player == null) return ApiError(400, "Could not identify player");

        var result = _lobbyService.JoinLobby(id, player.Id);

        return result switch
        {
            JoinLobbyResult.Success => Ok(),
            JoinLobbyResult.LobbyNotFound => ApiError(404, "Lobby not found"),
            JoinLobbyResult.AlreadyInLobby => ApiError(409, "You are already in this lobby"),
            JoinLobbyResult.LobbyFull => ApiError(400, "Lobby is full"),
            _ => ApiError(500, "Something went wrong")
        };
    }

    [Authorize]
    [HttpPost("{id:guid}/add-dummy")]
    public IActionResult AddDummy(Guid id)
    {
        if (!_env.IsDevelopment()) return ApiError(404, "Not found");

        return _lobbyService.AddDummyPlayer(id) switch
        {
            JoinLobbyResult.Success => Ok(),
            JoinLobbyResult.LobbyNotFound => ApiError(404, "Lobby not found"),
            JoinLobbyResult.LobbyFull => ApiError(400, "Lobby is full"),
            _ => ApiError(500, "Something went wrong")
        };
    }

    [Authorize]
    [HttpPost("{id:guid}/leave")]
    public IActionResult LeaveLobby(Guid id)
    {
        var player = GetCurrentPlayer();
        if (player == null) return ApiError(400, "Could not identify player");

        var result = _lobbyService.LeaveLobby(id, player.Id);

        return result switch
        {
            LeaveLobbyResult.Success => NoContent(),
            LeaveLobbyResult.LobbyNotFound => ApiError(404, "Lobby not found"),
            LeaveLobbyResult.NotInLobby => ApiError(409, "You are not in this lobby"),
            LeaveLobbyResult.GameInProgress => ApiError(400, "Cannot leave a game in progress"),
            _ => ApiError(500, "Something went wrong")
        };
    }
}
