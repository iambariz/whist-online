using WhistOnline.API.Models;

namespace WhistOnline.API.Services;

public class DeckService
{
    public List<Card> BuildDeck()
    {
        return Enum.GetValues<Suit>()
            .SelectMany(suit => Enum.GetValues<Rank>()
                .Select(rank => new Card { Suit = suit, Rank = rank }))
            .ToList();
    }

    public List<Card> Shuffle(List<Card> deck)
    {
        var rng = new Random();
        return deck.OrderBy(_ => rng.Next()).ToList();
    }

    public List<List<Card>> Deal(List<Card> deck, int playerCount, int cardsPerPlayer)
    {
        var hands = Enumerable.Range(0, playerCount)
            .Select(_ => new List<Card>())
            .ToList();

        for (int i = 0; i < cardsPerPlayer * playerCount; i++)
        {
            hands[i % playerCount].Add(deck[i]);
        }

        return hands;
    }
}