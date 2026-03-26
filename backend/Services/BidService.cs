using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Data;
using WhistOnline.API.DTOs;
using WhistOnline.API.Models;

namespace WhistOnline.API.Services;

public class BidService
{
    private readonly AppDbContext _db;

    public BidService(AppDbContext db)                                                                                                                      
    {
        _db = db;                                                                                                                                                                     
    }
    public Bid? SubmitBid(Guid gameId, Guid playerId, int amount)
    {
        var game = _db.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
                .ThenInclude(r => r.Bids)
            .FirstOrDefault(g => g.Id == gameId);

        if (game == null) return null;
        if (game.Status != GameStatus.Bidding) return null;
        if (game.Players.All(p => p.Id != playerId)) return null;

        var currentRound = game.Rounds.FirstOrDefault(r => r.RoundNumber == game.CurrentRound);

        if (currentRound == null) return null;
        if (!IsPlayersTurn(game, currentRound, playerId)) return null;
        if (HasPlayerBidAtCurrentRound(currentRound, playerId)) return null;

        return !IsBidValid(game, playerId, amount, currentRound) ? null : CreateBid(currentRound.Id, playerId, amount, game);
    }

    private bool HasPlayerBidAtCurrentRound(Round currentRound, Guid playerId)
    {
        return currentRound.Bids.Any(bid => bid.PlayerId == playerId);
    }

    // Bidding goes in order starting left of dealer, one at a time
    private bool IsPlayersTurn(Game game, Round currentRound, Guid playerId)
    {
        var expectedSeatIndex = (game.DealerIndex + 1 + currentRound.Bids.Count) % game.Players.Count;
        var expectedPlayer = game.Players.FirstOrDefault(p => p.SeatIndex == expectedSeatIndex);
        return expectedPlayer?.Id == playerId;
    }

    private bool IsBidValid(Game game, Guid playerId, int amount, Round currentRound)
    {
        // Bid cannot exceed cards dealt
        if (amount > currentRound.CardsDealt) return false;

        // Dealer's bid cannot make total equal cards dealt
        var dealerPlayer = game.Players.FirstOrDefault(p => p.SeatIndex == game.DealerIndex);
        if (dealerPlayer == null || dealerPlayer.Id != playerId) return true;

        var existingBidTotal = currentRound.Bids.Sum(b => b.Amount);
        return (existingBidTotal + amount) != currentRound.CardsDealt;
    }

    private Bid CreateBid(Guid currentRoundId, Guid playerId, int amount, Game game)
    {
        var bid = new Bid { Amount = amount, PlayerId = playerId, RoundId = currentRoundId };
        _db.Bids.Add(bid);
        game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % game.Players.Count;
        _db.SaveChanges();
        return bid;
    }
}