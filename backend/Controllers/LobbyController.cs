using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public IActionResult Create()
    {
        var player = GetCurrentPlayer();
        if (player == null) return BadRequest();

        var createdLobby = _lobbyService.CreateGameForPlayer(player);
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
        
        if (!_lobbyService.JoinLobby(id, player.Id)) return NotFound();

        return Ok();
    }

}