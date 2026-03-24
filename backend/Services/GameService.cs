using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.Models;

namespace WhistOnline.API.Services;

public class GameService
{
    private const int MinPlayers = 3;
    private const int MaxPlayers = 7;
    
    private readonly AppDbContext _db;
    private readonly DeckService _deckService;

    public GameService(AppDbContext db, DeckService deckService)                                                                                                                      
    {
        _db = db;                                                                                                                                                                     
        _deckService = deckService;    
    }
    public Game? StartGame(Guid gameId)
    {
        var game = _db.Games.Include(g => g.Players).FirstOrDefault(g => g.Id == gameId);                                                                                                     
        
        if (game == null) return null;
        if (game.Players.Count < MinPlayers || game.Players.Count > MaxPlayers) return null;

        DistributeHandsToPlayers(PrepareHands(game.Players), game.Players);
        
        game.Status = GameStatus.Bidding;
        _db.SaveChanges();                                                                                                                                                                    
        return game;
    }
    
    private List<List<Card>> PrepareHands(List<Player> players)                                                                                                                           
    {                                                         
        var deck = _deckService.TrimDeck(_deckService.BuildDeck(), players.Count);                                                                                                        
        var shuffled = _deckService.Shuffle(deck);
        return _deckService.Deal(shuffled, players.Count, shuffled.Count / players.Count);                                                                                                
    }     
    
    private void DistributeHandsToPlayers(List<List<Card>> hands, List<Player> players)                                                                                                   
    {                                      
        for (int i = 0; i < players.Count; i++)                                                                                                                                           
        {
            var player = players.First(p => p.SeatIndex == i);                                                                                                                            
            player.Hand = hands[i];                                                                                                                                                       
        }
    }     
}