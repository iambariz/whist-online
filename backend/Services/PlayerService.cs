using System.Security.Claims;
using WhistOnline.API.Data;
using WhistOnline.API.DTOs;
using WhistOnline.API.Models;

namespace WhistOnline.API.Services;

public class PlayerService
{
    private readonly AppDbContext _db;
    public PlayerService(AppDbContext db)                                                                                           
    {               
        _db = db;
    }              
    
    
    public Player? FindPlayerByGuid(Guid id)
    {
        return _db.Players.FirstOrDefault(p => p.Id == id);
    }

    public Player? CreatePlayer(CreatePlayerRequest request)
    {
        var newPlayer =  new Player { Name = request.Name };
        _db.Players.Add(newPlayer);
        _db.SaveChanges();
        return newPlayer;
    }
    
    public Player? GetPlayerFromToken(ClaimsPrincipal user)                                                                            
    {                                                                                                                                  
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);                                                                       
        if (id == null) return null;                                                                                                   
                                                                                                                                     
        return FindPlayerByGuid(Guid.Parse(id));
    }  
}