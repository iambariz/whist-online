using WhistOnline.API.DTOs;
using WhistOnline.API.Models;
using WhistOnline.API.Services;

namespace WhistOnline.API.Actions;

public class PlayCardAction : RoundAction
{
    private readonly PlayCardDto _playCardDto;
    private readonly TrickService _trickService;
    private readonly ScoringService _scoringService;
    private readonly GameService _gameService;

    public PlayCardAction(GameRules gameRules, PlayCardDto playCardDto, TrickService trickService, ScoringService scoringService, GameService gameService)
        : base(gameRules)
    {
        _playCardDto = playCardDto;
        _trickService = trickService;
        _scoringService = scoringService;
        _gameService = gameService;
    }

    protected override bool Validate(Game game, Player player)
    {
        if (game.Status != GameStatus.Playing) return false;
        if (!PlayerHasCard(player)) return false;

        var currentTrick = game.Rounds.Last().Tricks.LastOrDefault();
        if (currentTrick == null) return false;

        var card = new Card { Suit = _playCardDto.Suit, Rank = _playCardDto.Rank };
        return IsValidPlay(card, player, currentTrick);
    }

    protected override void ExecuteInternal(Game game, Player player)
    {
        var card = player.Hand.First(c => c.Suit == _playCardDto.Suit && c.Rank == _playCardDto.Rank);
        var currentTrick = game.Rounds.Last().Tricks.Last();
        currentTrick.CardsPlayed.Add(new CardPlay { PlayerId = player.Id, Card = card, TrickId = currentTrick.Id });
        if (currentTrick.LeadSuit == null) currentTrick.LeadSuit = card.Suit;
        player.Hand.Remove(card);
    }

     protected override void AfterAction(Game game)                                                                                                                                                                                                                                                                      
      {                                                                                                                                                                                                                                                                                                                   
          if (!IsTrickComplete(game)) return;                                                                                                                                                                                                                                                                             
          _trickService.EvaluateTrick(game, game.Rounds.Last().Tricks.Last());                                                                                                                                                                                                                                            
       
          // TODO: Create a new Trick for the next trick in this round

          if (!IsRoundComplete(game)) return;
          _scoringService.EvaluateRound(game);
          _gameService.AdvanceRound(game);
      }                                                                                                                                                                                                                                                                                                                   
                                                                
      private bool IsTrickComplete(Game game) =>
          game.Rounds.Last().Tricks.Last().CardsPlayed.Count == game.Players.Count;
                                                                                                                                                                                                                                                                                                                          
      private bool IsRoundComplete(Game game) =>
          game.Players.All(p => p.Hand.Count == 0);     

    private bool PlayerHasCard(Player player)
    {
        return player.Hand.Any(c => c.Suit == _playCardDto.Suit && c.Rank == _playCardDto.Rank);
    }

    private bool IsValidPlay(Card card, Player player, Trick currentTrick)
    {
        if (currentTrick.LeadSuit == null) return true;

        var hasLeadSuit = player.Hand.Any(c => c.Suit == currentTrick.LeadSuit);
        if (!hasLeadSuit) return true;

        return card.Suit == currentTrick.LeadSuit;
    }
}
