using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.DTOs;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : BaseController
{
    private readonly GameService _gameService;
    private readonly BidService _bidService;

    public GameController(GameService gameService, PlayerService playerService, BidService bidService)
        : base(playerService)
    {
        _gameService = gameService;
        _bidService = bidService;
    }
    
    [Authorize]
    [HttpGet("{id:guid}")]
    public IActionResult GetGameState(Guid id)
    {
        var player = GetCurrentPlayer();
        if (player == null) return BadRequest();

        var gameState = _gameService.GetGameState(id, player.Id);
        
        if (gameState == null) return NotFound();
        return Ok(gameState);
    }
    
    [Authorize]
    [HttpPost("{id:guid}/start")]
    public IActionResult StartGame(Guid id)
    {
        var player = GetCurrentPlayer();
        if (player == null) return BadRequest();

        if (_gameService.StartGame(id, player.Id) == null) return NotFound();
        var gameState = _gameService.GetGameState(id, player.Id);
        return Ok(gameState);
                                                                                                                                                        
    }
    
    [Authorize]
    [HttpPost("{id:guid}/bid")]
    public IActionResult SubmitBid(Guid id,[FromBody] SubmitBidDto submitBidDto)
    {
        var player = GetCurrentPlayer();
        if (player == null) return BadRequest();

        var bid = _bidService.SubmitBid(id, player.Id, submitBidDto.Amount);
        if (bid == null) return BadRequest();

        return Ok(bid);
    }
    
    [Authorize]
    [HttpPost("{id:guid}/play")]
    public IActionResult PlayCard(Guid id)
    {
        return Ok();
    }
    
    [Authorize]
    [HttpGet("{id:guid}/scores")]
    public IActionResult GetScoreBoard(Guid id)
    {
        return Ok();
    }
}
