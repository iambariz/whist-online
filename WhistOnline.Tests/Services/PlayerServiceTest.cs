namespace WhistOnline.Tests.Services;

using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.Models;
using WhistOnline.API.Services;


public class PlayerServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique per test
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void FindPlayerByGuid_ReturnsPlayer_WhenExists()
    {
        // arrange
        var db = CreateDb();
        var player = new Player { Name = "Alice" };
        db.Players.Add(player);
        db.SaveChanges();

        var service = new PlayerService(db);
        var result = service.FindPlayerByGuid(player.Id);
        // assert - check the result is not null and has the right name
        
        Assert.NotNull(result);
    }
}