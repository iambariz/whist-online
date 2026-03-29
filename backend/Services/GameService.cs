using WhistOnline.API.DTOs;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;

namespace WhistOnline.API.Services;

public class GameService
{
    private const int MinPlayers = 3;
    private const int MaxPlayers = 7;

    private readonly GameRepository _gameRepository;
    private readonly DeckService _deckService;

    public GameService(GameRepository gameRepository, DeckService deckService)
    {
        _gameRepository = gameRepository;
        _deckService = deckService;
    }

    public Game? StartGame(Guid gameId, Guid playerId)
    {
        var game = _gameRepository.FindByIdWithPlayers(gameId);
        if (game == null) return null;
        if (!ValidateGameStart(game, playerId)) return null;

        DistributeHandsToPlayers(PrepareHands(game.Players), game.Players);

        game.Status = GameStatus.Bidding;
        _gameRepository.SaveChanges();
        return game;
    }

    public GameStateDto? GetGameState(Guid gameId, Guid playerId)
    {
        var game = _gameRepository.FindByIdWithPlayers(gameId);
        if (game == null) return null;
        if (!IsPlayerInGame(game, playerId)) return null;

        return new GameStateDto
        {
            GameId = game.Id,
            Status = game.Status.ToString(),
            CurrentRound = game.CurrentRound,
            TotalRounds = game.TotalRounds,
            TrumpSuit = game.TrumpSuit?.ToString(),
            CurrentPlayerIndex = game.CurrentPlayerIndex,
            DealerIndex = game.DealerIndex,
            Players = game.Players.Select(p => new PlayerSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                SeatIndex = p.SeatIndex,
                CardCount = p.Hand.Count,
                Score = p.Score
            }).ToList(),
            MyHand = game.Players.FirstOrDefault(p => p.Id == playerId)?.Hand
        };
    }

    public void AdvanceRound(Game game)
    {
        if (game.CurrentRound == game.TotalRounds)
        {
            game.Status = GameStatus.Finished;
        }
        else
        {
            game.CurrentRound++;
            RotateTrumpSuit(game);
        }
    }

    private void RotateTrumpSuit(Game game)
    {
        var suits = Enum.GetValues<Suit>();
        var currentIndex = game.TrumpSuit.HasValue ? Array.IndexOf(suits, game.TrumpSuit.Value) : -1;                                                                                                                                                                                                                   
        game.TrumpSuit = suits[(currentIndex + 1) % suits.Length];                                                                                                                                                                                                                                                      
    }                                                                                                                                                                                                                                                                                                                   

    private bool ValidateGameStart(Game game, Guid playerId)
    {
        return game.Players.Any(p => p.Id == playerId) &&
               game.Players.Count >= MinPlayers &&
               game.Players.Count <= MaxPlayers;
    }

    private bool IsPlayerInGame(Game game, Guid playerId)
    {
        return game.Players.Any(p => p.Id == playerId);
    }

    private List<List<Card>> PrepareHands(List<Player> players)
    {
        var deck = _deckService.TrimDeck(_deckService.BuildDeck(), players.Count);
        var shuffled = _deckService.Shuffle(deck);
        return _deckService.Deal(shuffled, players.Count, shuffled.Count / players.Count);
    }

    private void DistributeHandsToPlayers(List<List<Card>> hands, List<Player> players)
    {
        var playerBySeat = players.ToDictionary(p => p.SeatIndex);
        for (int i = 0; i < players.Count; i++)
        {
            playerBySeat[i].Hand = hands[i];
        }
    }
}
