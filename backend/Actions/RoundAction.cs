using WhistOnline.API.Models;

namespace WhistOnline.API.Actions;

public abstract class RoundAction
{
    private readonly GameRules _gameRules;

    protected RoundAction(GameRules gameRules)
    {
        _gameRules = gameRules;
    }

    public bool Execute(Game game, Player player)
    {
        if (!_gameRules.IsPlayersTurn(game, player)) return false;
        if (!Validate(game, player)) return false;

        ExecuteInternal(game, player);
        AdvancePlayerIndex(game);
        AfterAction(game);

        return true;
    }

    protected abstract bool Validate(Game game, Player player);
    protected abstract void ExecuteInternal(Game game, Player player);
    protected virtual void AfterAction(Game game) { }

    private void AdvancePlayerIndex(Game game)
    {
        game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % game.Players.Count;
    }
}
