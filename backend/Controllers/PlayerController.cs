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
    private readonly TokenService _tokenService;
    public PlayerController(PlayerService playerService, TokenService tokenService)                                                                                           
    {               
        _playerService = playerService;
        _tokenService = tokenService;
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
        
        if(playerQuery == null) return BadRequest();
        
        var token = _tokenService.GenerateToken(playerQuery);                         
        return CreatedAtAction(nameof(GetPlayer), new { id = playerQuery.Id }, new {  
            player = playerQuery, token });   
    }
}