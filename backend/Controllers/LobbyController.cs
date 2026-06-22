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

    public LobbyController(LobbyService lobbyService, PlayerService playerService)
        : base(playerService)
    {
        _lobbyService = lobbyService;
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
}
