using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.Actions;
using WhistOnline.API.DTOs;
using WhistOnline.API.Repositories;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : BaseController
{
    private readonly GameService _gameService;
    private readonly GameRepository _gameRepository;
    private readonly BidRepository _bidRepository;
    private readonly GameRules _gameRules;

    public GameController(
        GameService gameService,
        PlayerService playerService,
        GameRepository gameRepository,
        BidRepository bidRepository,
        GameRules gameRules)
        : base(playerService)
    {
        _gameService = gameService;
        _gameRepository = gameRepository;
        _bidRepository = bidRepository;
        _gameRules = gameRules;
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

        var game = _gameRepository.FindByIdWithPlayersAndRoundsAndBids(id);
        if (game == null) return NotFound();

        var action = new BidAction(_gameRules, _bidRepository, submitBidDto.Amount);
        if (!action.Execute(game, player)) return BadRequest();

        _gameRepository.SaveChanges();
        return Ok(action.Result);
    }
    
    [Authorize]
    [HttpPost("{id:guid}/play")]
    public IActionResult PlayCard(Guid id, [FromBody] PlayCardDto cardDto)
    {
        var player = GetCurrentPlayer();
        if (player == null) return BadRequest();

        var game = _gameRepository.FindByIdWithRoundsAndTricks(id);
        if (game == null) return NotFound();

        var action = new PlayCardAction(_gameRules, cardDto);
        if (!action.Execute(game, player)) return BadRequest();

        _gameRepository.SaveChanges();
        var gameState = _gameService.GetGameState(id, player.Id);
        return Ok(gameState);
    }
    
    [Authorize]
    [HttpGet("{id:guid}/scores")]
    public IActionResult GetScoreBoard(Guid id)
    {
        return Ok();
    }
}
