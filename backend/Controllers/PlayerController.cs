using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.Data;
using WhistOnline.API.DTOs;
using WhistOnline.API.Models;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly PlayerService _playerService;
    public PlayerController(PlayerService playerService)                                                                                           
    {               
        _playerService = playerService;
    }              
    
    [HttpGet("{id}")]                                                                                                                  
    public IActionResult GetPlayer(Guid id)                                                                                            
    {                    
        var playerQuery = _playerService.FindPlayerByGuid(id);
        
        return playerQuery != null ? Ok(playerQuery) : NotFound();
    }  

    [HttpPost()]
    public IActionResult AddPlayer([FromBody] CreatePlayerRequest player)
    {
        var playerQuery = _playerService.CreatePlayer(player);
        
        return playerQuery != null ? 
            CreatedAtAction(nameof(GetPlayer), new { id = playerQuery.Id }, playerQuery) : 
            BadRequest();
    }
}