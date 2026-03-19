using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.Models;

namespace WhistOnline.API.Services;

public class LobbyService
{
    private readonly AppDbContext _db;
    public LobbyService(AppDbContext db)                                                                                           
    {               
        _db = db;
    }              
    
    
    public Game? FindLobbyById(Guid id)
    {
        return _db.Games.FirstOrDefault(p => p.Id == id);
    }

    public List<Game> FindOpenLobbies()                                                                                                
    {
        return _db.Games.Where(g => g.Status == GameStatus.Waiting).ToList();                                                          
    }

    //Todo: Rate limiting
    public Game CreateGameForPlayer(Player player)
    {
        player.SeatIndex = 0;
        var newGame = new Game { Players = new List<Player> { player } };                                                              
        _db.Games.Add(newGame);
        _db.SaveChanges();                                                                                                             
        return newGame;
    }

    //Todo: JWT + User check
    public bool DeleteGame(Guid id)
    {
        var game = _db.Games.FirstOrDefault(p => p.Id == id);
        if (game == null) return false;
        
        _db.Games.Remove(game);
        _db.SaveChanges();
        return true;
    }
    
    public bool JoinLobby(Guid id)
    {
        var playerId = 123;
        var player = _db.Players.Find(playerId);                                                                                           
        if (player == null) return false;

        var lobby = _db.Games                                                                                                              
            .Include(g => g.Players)                                                                                                       
            .FirstOrDefault(g => g.Id == id && g.Status == GameStatus.Waiting);                                                            

        if (lobby == null || lobby.Players.Any(p => p.Id == player.Id)) return false;
        
        lobby.Players.Add(player);             
        
        _db.SaveChanges();
        return true;
    }
}