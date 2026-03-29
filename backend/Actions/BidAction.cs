using WhistOnline.API.Models;
using WhistOnline.API.Repositories;

namespace WhistOnline.API.Actions;

public class BidAction : RoundAction
{
    private readonly BidRepository _bidRepository;
    private readonly int _amount;

    public Bid? Result { get; private set; }

    public BidAction(GameRules gameRules, BidRepository bidRepository, int amount)
        : base(gameRules)
    {
        _bidRepository = bidRepository;
        _amount = amount;
    }

    protected override bool Validate(Game game, Player player)
    {
        if (game.Status != GameStatus.Bidding) return false;

        var currentRound = GetCurrentRound(game);
        if (currentRound == null) return false;
        if (currentRound.Bids.Any(b => b.PlayerId == player.Id)) return false;
        if (_amount > currentRound.CardsDealt) return false;

        // Dealer's bid cannot make total equal cards dealt
        var dealerPlayer = game.Players.FirstOrDefault(p => p.SeatIndex == game.DealerIndex);
        if (dealerPlayer?.Id == player.Id)
        {
            var existingBidTotal = currentRound.Bids.Sum(b => b.Amount);
            if (existingBidTotal + _amount == currentRound.CardsDealt) return false;
        }

        return true;
    }

    protected override void ExecuteInternal(Game game, Player player)
    {
        var currentRound = GetCurrentRound(game)!;
        var bid = new Bid { Amount = _amount, PlayerId = player.Id, RoundId = currentRound.Id };
        currentRound.Bids.Add(bid);
        _bidRepository.Add(bid);
        Result = bid;
    }

    protected override void AfterAction(Game game)
    {
        var currentRound = GetCurrentRound(game)!;
        if (currentRound.Bids.Count < game.Players.Count) return;
        game.Status = GameStatus.Playing;
        currentRound.Tricks.Add(new Trick                                                                                                  
        {                                                                                                                                  
            TrickNumber = currentRound.Tricks.Count + 1,                                                                                   
            RoundId = currentRound.Id                             
        });
    }

    private Round? GetCurrentRound(Game game) => game.Rounds.FirstOrDefault(r => r.RoundNumber == game.CurrentRound);
}
