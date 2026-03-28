using WhistOnline.API.Models;

namespace WhistOnline.API.Actions;

public class GameRules
{
    public bool IsPlayersTurn(Game game, Player player)
    {
        return game.CurrentPlayerIndex == player.SeatIndex;
    }
}
