namespace WhistOnline.API.Models;

public class Round
{
    public Guid Id { get; set; }
    public int RoundNumber { get; set; }
    public int CardsDealt { get; set; }
    public Suit? TrumpSuit { get; set; }
    public int BidTotal { get; set; }

    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;

    public List<Bid> Bids { get; set; } = [];
    public List<Trick> Tricks { get; set; } = [];
}