using WhistOnline.API.DTOs;
using WhistOnline.API.Models;

namespace WhistOnline.API.Actions;

public class PlayCardAction : RoundAction
{
    private readonly PlayCardDto _playCardDto;

    public PlayCardAction(GameRules gameRules, PlayCardDto playCardDto)
        : base(gameRules)
    {
        _playCardDto = playCardDto;
    }

    protected override bool Validate(Game game, Player player)
    {
        if (game.Status != GameStatus.Playing) return false;
        if (!PlayerHasCard(player)) return false;

        var currentTrick = game.Rounds.Last().Tricks.LastOrDefault();
        if (currentTrick == null) return false;

        var card = new Card { Suit = _playCardDto.Suit, Rank = _playCardDto.Rank };
        return IsValidPlay(card, player, currentTrick);
    }

    protected override void ExecuteInternal(Game game, Player player)
    {
        // TODO: 1. Find the card in the player's hand
        // TODO: 2. Add CardPlay to current trick's CardsPlayed
        // TODO: 3. Set trick's LeadSuit if first card
        // TODO: 4. Remove card from player's hand
    }

    protected override void AfterAction(Game game)
    {
        // TODO: Call EvaluateTrick if all players have played
    }

    private bool PlayerHasCard(Player player)
    {
        return player.Hand.Any(c => c.Suit == _playCardDto.Suit && c.Rank == _playCardDto.Rank);
    }

    private bool IsValidPlay(Card card, Player player, Trick currentTrick)
    {
        if (currentTrick.LeadSuit == null) return true;

        var hasLeadSuit = player.Hand.Any(c => c.Suit == currentTrick.LeadSuit);
        if (!hasLeadSuit) return true;

        return card.Suit == currentTrick.LeadSuit;
    }
}
