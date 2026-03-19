namespace WhistOnline.API.Models;

public enum GameStatus
{
    Waiting,
    Bidding,
    Playing,
    Scoring,
    Finished
}

public class Game
{
    public Guid Id { get; set; }
    public GameStatus Status { get; set; } = GameStatus.Waiting;                                                                       
    public int CurrentRound { get; set; }
    public int TotalRounds { get; set; }
    public Suit? TrumpSuit { get; set; }
    public int DealerIndex { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Player> Players { get; set; } = [];
    public List<Round> Rounds { get; set; } = [];
}