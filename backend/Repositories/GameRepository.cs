using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.Models;

namespace WhistOnline.API.Repositories;

public class GameRepository
{
    private readonly AppDbContext _db;

    public GameRepository(AppDbContext db)
    {
        _db = db;
    }

    public Game? FindById(Guid id)
    {
        return _db.Games.FirstOrDefault(g => g.Id == id);
    }

    public Game? FindByIdWithPlayers(Guid id)
    {
        return _db.Games.Include(g => g.Players).FirstOrDefault(g => g.Id == id);
    }

    public Game? FindByIdWithRoundsAndTricks(Guid id)
    {
        return _db.Games                                                                                                                                                                                                                                                                                                
            .Include(g => g.Players)                                                                                                                                                                                                                                                                                    
            .Include(g => g.Rounds)                                                                                                                                                                                                                                                                                     
            .ThenInclude(r => r.Tricks)
            .ThenInclude(t => t.CardsPlayed)                                                                                                                                                                                                                                                                    
            .FirstOrDefault(g => g.Id == id);
    }

    public Game? FindByIdWithPlayersAndRoundsAndBids(Guid id)
    {
        return _db.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
                .ThenInclude(r => r.Bids)
            .FirstOrDefault(g => g.Id == id);
    }

    public List<Game> FindOpenLobbies()
    {
        return _db.Games.Where(g => g.Status == GameStatus.Waiting).ToList();
    }

    public Game? FindOpenLobbyByIdWithPlayers(Guid id)
    {
        return _db.Games
            .Include(g => g.Players)
            .FirstOrDefault(g => g.Id == id && g.Status == GameStatus.Waiting);
    }

    public void Add(Game game)
    {
        _db.Games.Add(game);
    }

    public void Remove(Game game)
    {
        _db.Games.Remove(game);
    }

    public void SaveChanges()
    {
        _db.SaveChanges();
    }
}
