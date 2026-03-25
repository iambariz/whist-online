using WhistOnline.API.Models;

namespace WhistOnline.API.DTOs;

public class GameStateDto                                                     
{
    public Guid GameId { get; set; }                                          
    public string? Status { get; set; }
    public int CurrentRound { get; set; }
    public int TotalRounds { get; set; }                                      
    public string? TrumpSuit { get; set; }
    public int CurrentPlayerIndex { get; set; }                               
    public int DealerIndex { get; set; }
    public List<PlayerSummaryDto>? Players { get; set; }                       
    public List<Card>? MyHand { get; set; }                                    
}
