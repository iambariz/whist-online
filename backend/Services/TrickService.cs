using WhistOnline.API.Models;

namespace WhistOnline.API.Services;

public class TrickService
{
public void EvaluateTrick(Game game, Trick currentTrick)
    {
      var winner = DetermineWinner(game, currentTrick);                                                                                                                                                                                                                                                               
      if (winner == null) return;                                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                                                                      
      currentTrick.WinnerPlayerId = winner.PlayerId;                                                                                                                                                                                                                                                                  
      game.CurrentPlayerIndex = game.Players.First(p => p.Id == winner.PlayerId).SeatIndex;
    }                                                                                                                                                                                                                                                                                                                   
    
    private CardPlay? DetermineWinner(Game game, Trick currentTrick)                                                                                                                                                                                                                                                    
    {                                                         
      return currentTrick.CardsPlayed
                 .Where(cp => cp.Card.Suit == game.TrumpSuit)                                                                                                                                                                                                                                                         
                 .MaxBy(cp => cp.Card.Rank)
             ?? currentTrick.CardsPlayed                                                                                                                                                                                                                                                                              
                 .Where(cp => cp.Card.Suit == currentTrick.LeadSuit)
                 .MaxBy(cp => cp.Card.Rank);                                                                                                                                                                                                                                                                          
    }
  
}
