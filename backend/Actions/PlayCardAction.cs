using WhistOnline.API.DTOs;
using WhistOnline.API.Models;

namespace WhistOnline.API.Actions;

public class PlayCardAction : RoundAction
{
    private readonly PlayCardDto _playCardDto;
    private readonly User

    public PlayCardAction(GameRules gameRules, PlayCardDto playCardDto)
        : base(gameRules)
    {
        _playCardDto = playCardDto;
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
        var currentTrick = game.Rounds.Last().Tricks.Last();
        if (currentTrick.CardsPlayed.Count == game.Players.Count)
        {                                                                                                                              
            EvaluateTrick(game, currentTrick);
            EvaluateRound(game, currentTrick);
            // TODO: EvaluateRound if no cards left                                                                                    
        }                                                                                                                              
    }

    
    private void EvaluateTrick(Game game, Trick currentTrick)                                                                          
    {
        var playedCards = currentTrick.CardsPlayed;                                                                                    
        var winner = playedCards                                                                                                       
                         .Where(cp => cp.Card.Suit == game.TrumpSuit)
                         .MaxBy(cp => cp.Card.Rank)                                                                                                 
                     ?? playedCards                                                                                                           
                         .Where(cp => cp.Card.Suit == currentTrick.LeadSuit)
                         .MaxBy(cp => cp.Card.Rank);                                                                                            
   
        if (winner == null) return;                                                                                                    
                                                                                                                                   
        currentTrick.WinnerPlayerId = winner.PlayerId;                                                                                 
        var winnerPlayer = game.Players.First(p => p.Id == winner.PlayerId);
        game.CurrentPlayerIndex = winnerPlayer.SeatIndex;                                                                              
    }

    private void EvaluateRound(Game game)
    {
        
    }

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
