using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.DTOs;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LobbyController : ControllerBase
{
    private readonly LobbyService _lobbyService;
    private readonly PlayerService _playerService;
    public LobbyController(LobbyService lobbyService, PlayerService playerService)
    {
        _lobbyService = lobbyService;
        _playerService = playerService;
    }
    
    [HttpGet]
    public IActionResult GetLobbies()
    {
        var lobbies = _lobbyService.FindOpenLobbies();
        
        return Ok(lobbies);
    }
    
    [HttpPost]
    public IActionResult Create([FromBody] CreateLobbyDto dto)
    {
        var player = _playerService.FindPlayerByGuid(dto.Id);

        if (player == null)
        {
            return BadRequest();
        }

        var createdLobby = _lobbyService.CreateGameForPlayer(player);
        return Ok(createdLobby);
    }
    
    [HttpDelete("{id:guid}")]
    public IActionResult DeleteLobby(Guid id)
    {
        if (!_lobbyService.DeleteGame(id)) return NotFound();
        return NoContent();                                                                                                                
    }
    
    [Authorize]
    [HttpPost("{id:guid}/join")]
    public IActionResult JoinLobby(Guid id)
    {
        //Add JWT service in next commit
        
        if (!_lobbyService.JoinLobby(id)) return NotFound();

        return Ok();
    }

}