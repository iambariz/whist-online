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
    
    [Authorize]
    [HttpPost]
    public IActionResult Create([FromBody] CreateLobbyRequest request)
    {
        var player = GetCurrentPlayer();
        if (player == null) return BadRequest();

        var createdLobby = _lobbyService.CreateGameForPlayer(player, request.Name);
        return Ok(createdLobby);
    }
    
    [HttpDelete("{id:guid}")]
    public IActionResult DeleteLobby(Guid id)
    {
        //Todo: Validation here
        if (!_lobbyService.DeleteGame(id)) return NotFound();
        return NoContent();                                                                                                                
    }
    
    [Authorize]
    [HttpPost("{id:guid}/join")]
    public IActionResult JoinLobby(Guid id)
    {
        var player = GetCurrentPlayer();
        if (player == null) return BadRequest();
        
        var result = _lobbyService.JoinLobby(id, player.Id);

        return result switch
        {
            JoinLobbyResult.Success => Ok(),
            JoinLobbyResult.LobbyNotFound => NotFound("Lobby not found"),
            JoinLobbyResult.AlreadyInLobby => Conflict("Already in lobby"),
            JoinLobbyResult.LobbyFull => BadRequest("Lobby is full"),
            _ => StatusCode(500)
        };
    }

}