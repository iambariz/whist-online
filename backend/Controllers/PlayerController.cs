using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.DTOs;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/players")]
public class PlayerController : BaseController
{
    private readonly TokenService _tokenService;

    public PlayerController(PlayerService playerService, TokenService tokenService)
        : base(playerService)
    {
        _tokenService = tokenService;
    }
    
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var player = GetCurrentPlayer();
        return player != null ? Ok(player) : Unauthorized();
    }

    [HttpGet("{id}")]
    public IActionResult GetPlayer(Guid id)
    {
        var player = _playerService.FindPlayerByGuid(id);
        return player != null ? Ok(player) : NotFound();
    }
    
    [HttpPost]
    public IActionResult AddPlayer([FromBody] CreatePlayerRequest player)
    {
        var created = _playerService.CreatePlayer(player);
        if (created == null) return BadRequest();

        var token = _tokenService.GenerateToken(created);
        return CreatedAtAction(nameof(GetPlayer), new { id = created.Id }, new { player = created, token });
    }
}