using WhistOnline.API.Data;
using WhistOnline.API.Models;

namespace WhistOnline.API.Repositories;

public class PlayerRepository
{
    private readonly AppDbContext _db;

    public PlayerRepository(AppDbContext db)
    {
        _db = db;
    }

    public Player? FindById(Guid id)
    {
        return _db.Players.FirstOrDefault(p => p.Id == id);
    }

    public Player? FindByIdTracked(Guid id)
    {
        return _db.Players.Find(id);
    }

    public void Add(Player player)
    {
        _db.Players.Add(player);
    }

    public void SaveChanges()
    {
        _db.SaveChanges();
    }
}
