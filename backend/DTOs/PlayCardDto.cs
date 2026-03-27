using WhistOnline.API.Models;

namespace WhistOnline.API.DTOs;

public class PlayCardDto                                                                                                           
{                                                                                                                                  
    public required Suit Suit { get; set; }                                                                                        
    public required Rank Rank { get; set; }
}
