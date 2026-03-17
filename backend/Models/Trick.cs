public class CardPlay
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Card Card { get; set; } = null!;

    public Guid TrickId { get; set; }
    public Trick Trick { get; set; } = null!;
}

public class Trick
{
    public Guid Id { get; set; }
    public int TrickNumber { get; set; }
    public Suit? LeadSuit { get; set; }
    public Guid? WinnerPlayerId { get; set; }

    public Guid RoundId { get; set; }
    public Round Round { get; set; } = null!;

    public List<CardPlay> CardsPlayed { get; set; } = [];
}