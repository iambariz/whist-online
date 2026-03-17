namespace WhistOnline.API.Models;

public class Bid
{
    public Guid Id { get; set; }
    public int Amount { get; set; }
    public int TricksWon { get; set; }

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public Guid RoundId { get; set; }
    public Round Round { get; set; } = null!;
}