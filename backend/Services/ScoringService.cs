using WhistOnline.API.Models;

namespace WhistOnline.API.Services;

public class ScoringService
{
    public void EvaluateRound(Game game)
    {
        var round = game.Rounds.Last();
        ScorePlayers(game, round);
    }                                                         
                                                                                                                                                                                                                                                                                                                          
      private void ScorePlayers(Game game, Round round)         
      {
          var tricksWonByPlayer = round.Tricks
              .Where(t => t.WinnerPlayerId != null)                                                                                                                                                                                                                                                                       
              .GroupBy(t => t.WinnerPlayerId!.Value)
              .ToDictionary(g => g.Key, g => g.Count());                                                                                                                                                                                                                                                                  
                                                                                                                                                                                                                                                                                                                          
          foreach (var bid in round.Bids)
          {                                                                                                                                                                                                                                                                                                               
              var tricksWon = tricksWonByPlayer.GetValueOrDefault(bid.PlayerId, 0);
              var player = game.Players.First(p => p.Id == bid.PlayerId);
                                                                                                                                                                                                                                                                                                                          
              if (tricksWon == bid.Amount)
                  player.Score += 10 + bid.Amount;                                                                                                                                                                                                                                                                        
              else                                              
                  player.Score -= Math.Abs(tricksWon - bid.Amount);
          }
      }
    
}
