namespace WhistOnline.API.Models;

public class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public int SeatIndex { get; set; }
    public bool IsConnected { get; set; }

    public Guid? GameId { get; set; }
    public Game? Game { get; set; }

    public List<Card> Hand { get; set; } = [];
}