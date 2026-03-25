using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.DTOs;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly PlayerService _playerService;
    public GameController(GameService gameService,  PlayerService playerService)
    {
        _gameService = gameService;
        _playerService = playerService;
    }

    [Authorize]
    [HttpPost("{id:guid}/start")]
    public IActionResult StartGame(Guid id)
    {
        var player = _playerService.GetPlayerFromToken(User);
        if (player == null) return BadRequest();
        
        var game = _gameService.StartGame(id, player.Id);
        if (game == null) return NotFound();
        return Ok(game);
                                                                                                                                                        
    }
}
