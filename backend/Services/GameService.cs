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

        InitGame(game);
        SetupRound(game, MaxCardsPerHand(game.Players.Count));

        game.Status = GameStatus.Bidding;
        _gameRepository.SaveChanges();
        return game;
    }

    public bool PlayerBelongsToGame(Guid gameId, Guid playerId)
    {
        var game = _gameRepository.FindByIdWithPlayers(gameId);
        if (game == null) return false;
        return IsPlayerInGame(game, playerId);
    }

    private void InitGame(Game game)
    {
        // One round per trump suit, plus a final no-trump round.
        game.TotalRounds = Enum.GetValues<Suit>().Length + 1;
        game.CurrentRound = 1;
        game.TrumpSuit = Suit.Clubs;
        game.DealerIndex = 0;
        game.CurrentPlayerIndex = 1 % game.Players.Count;
    }

    private int MaxCardsPerHand(int playerCount) =>
        _deckService.TrimDeck(_deckService.BuildDeck(), playerCount).Count / playerCount;

    private void SetupRound(Game game, int cardsDealt)
    {
        game.Rounds.Add(new Round
        {
            RoundNumber = game.CurrentRound,
            CardsDealt = cardsDealt,
            TrumpSuit = game.TrumpSuit
        });
        DistributeHandsToPlayers(PrepareHands(game.Players, cardsDealt), game.Players);
    }

    public GameStateDto? GetGameState(Guid gameId, Guid playerId)
    {
        var game = _gameRepository.FindByIdWithPlayers(gameId);
        if (game == null) return null;
        if (!IsPlayerInGame(game, playerId)) return null;

        return new GameStateDto
        {
            GameId = game.Id,
            HostPlayerId = game.HostPlayerId,
            Status = game.Status.ToString(),
            CurrentRound = game.CurrentRound,
            TotalRounds = game.TotalRounds,
            TrumpSuit = game.TrumpSuit?.ToString(),
            CurrentPlayerIndex = game.CurrentPlayerIndex,
            DealerIndex = game.DealerIndex,
            MinPlayers = MinPlayers,
            MaxPlayers = MaxPlayers,
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
            game.DealerIndex = (game.DealerIndex + 1) % game.Players.Count;
            game.CurrentPlayerIndex = (game.DealerIndex + 1) % game.Players.Count;

            SetupRound(game, MaxCardsPerHand(game.Players.Count));
            game.Status = GameStatus.Bidding;
        }
    }

    private void RotateTrumpSuit(Game game)
    {
        var suits = Enum.GetValues<Suit>();
        if (!game.TrumpSuit.HasValue)
        {
            game.TrumpSuit = suits[0];
            return;
        }
        var currentIndex = Array.IndexOf(suits, game.TrumpSuit.Value);
        game.TrumpSuit = currentIndex == suits.Length - 1
            ? null
            : suits[currentIndex + 1];
    }

    private bool ValidateGameStart(Game game, Guid playerId)
    {
        return game.HostPlayerId == playerId &&
               game.Players.Count >= MinPlayers &&
               game.Players.Count <= MaxPlayers;
    }

    private bool IsPlayerInGame(Game game, Guid playerId)
    {
        return game.Players.Any(p => p.Id == playerId);
    }

    private List<List<Card>> PrepareHands(List<Player> players, int cardsPerPlayer)
    {
        var deck = _deckService.TrimDeck(_deckService.BuildDeck(), players.Count);
        var shuffled = _deckService.Shuffle(deck);
        return _deckService.Deal(shuffled, players.Count, cardsPerPlayer);
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
