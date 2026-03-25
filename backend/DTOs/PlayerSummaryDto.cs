using WhistOnline.API.Models;

namespace WhistOnline.API.DTOs;

public class PlayerSummaryDto                                                 
{               
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int SeatIndex { get; set; }                                        
    public int CardCount { get; set; }
    public int Score { get; set; }                                            
}     