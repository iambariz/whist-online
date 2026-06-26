using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WhistOnline.API.Actions;
using WhistOnline.API.DTOs;
using WhistOnline.API.Hubs;
using WhistOnline.API.Repositories;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/games")]
public class GameController : BaseController
{
    private readonly GameService _gameService;
    private readonly GameRepository _gameRepository;
    private readonly BidRepository _bidRepository;
    private readonly GameRules _gameRules;
    private readonly TrickService _trickService;
    private readonly ScoringService _scoringService;
    private readonly ScoreBoardService _scoreBoardService;
    private readonly IHubContext<GameHub> _gameHub;

    public GameController(
        GameService gameService,
        PlayerService playerService,
        GameRepository gameRepository,
        BidRepository bidRepository,
        GameRules gameRules,
        TrickService trickService,
        ScoringService scoringService,
        ScoreBoardService scoreBoardService,
        IHubContext<GameHub> gameHub)
        : base(playerService)
    {
        _gameService = gameService;
        _gameRepository = gameRepository;
        _bidRepository = bidRepository;
        _gameRules = gameRules;
        _trickService = trickService;
        _scoringService = scoringService;
        _scoreBoardService = scoreBoardService;
        _gameHub = gameHub;
    }

    private Task NotifyGameUpdated(Guid gameId) =>
        _gameHub.Clients.Group(GameHub.GroupName(gameId)).SendAsync("GameUpdated", gameId);

    [Authorize]
    [HttpGet("{id:guid}")]
    public IActionResult GetGameState(Guid id)
    {
        var player = GetCurrentPlayer();
        if (player == null) return ApiError(400, "Could not identify player");

        var gameState = _gameService.GetGameState(id, player.Id);
        if (gameState == null) return ApiError(404, "Game not found");
        return Ok(gameState);
    }

    [Authorize]
    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> StartGame(Guid id)
    {
        var player = GetCurrentPlayer();
        if (player == null) return ApiError(400, "Could not identify player");

        if (_gameService.StartGame(id, player.Id) == null) return ApiError(404, "Game not found");
        var gameState = _gameService.GetGameState(id, player.Id);
        await NotifyGameUpdated(id);
        return Ok(gameState);
    }

    [Authorize]
    [HttpPost("{id:guid}/bid")]
    public async Task<IActionResult> SubmitBid(Guid id, [FromBody] SubmitBidDto submitBidDto)
    {
        var player = GetCurrentPlayer();
        if (player == null) return ApiError(400, "Could not identify player");

        var game = _gameRepository.FindByIdWithPlayersAndRoundsAndBids(id);
        if (game == null) return ApiError(404, "Game not found");

        var action = new BidAction(_gameRules, _bidRepository, submitBidDto.Amount);
        if (!action.Execute(game, player)) return ApiError(400, "Invalid bid");

        _gameRepository.SaveChanges();
        await NotifyGameUpdated(id);
        return Ok(action.Result);
    }

    [Authorize]
    [HttpPost("{id:guid}/play")]
    public async Task<IActionResult> PlayCard(Guid id, [FromBody] PlayCardDto cardDto)
    {
        var player = GetCurrentPlayer();
        if (player == null) return ApiError(400, "Could not identify player");

        var game = _gameRepository.FindByIdWithRoundsAndTricks(id);
        if (game == null) return ApiError(404, "Game not found");

        var action = new PlayCardAction(_gameRules, cardDto, _trickService, _scoringService, _gameService);
        if (!action.Execute(game, player)) return ApiError(400, "Invalid card play");

        _gameRepository.SaveChanges();
        var gameState = _gameService.GetGameState(id, player.Id);
        await NotifyGameUpdated(id);
        return Ok(gameState);
    }

    [Authorize]
    [HttpGet("{id:guid}/scoreboard")]
    public IActionResult GetScoreBoard(Guid id)
    {
        var player = GetCurrentPlayer();
        if (player == null) return ApiError(400, "Could not identify player");

        var game = _gameRepository.FindByIdWithRoundsAndTricks(id);
        if (game == null) return ApiError(404, "Game not found");

        return Ok(_scoreBoardService.GetScoreList(game));
    }
}
