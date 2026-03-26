using WhistOnline.API.Data;
using WhistOnline.API.Models;

namespace WhistOnline.API.Repositories;

public class BidRepository
{
    private readonly AppDbContext _db;

    public BidRepository(AppDbContext db)
    {
        _db = db;
    }

    public void Add(Bid bid)
    {
        _db.Bids.Add(bid);
    }

    public void SaveChanges()
    {
        _db.SaveChanges();
    }
}
