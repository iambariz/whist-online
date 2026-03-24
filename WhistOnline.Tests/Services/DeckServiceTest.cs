namespace WhistOnline.Tests.Services;

using WhistOnline.API.Models;
using WhistOnline.API.Services;

public class DeckServiceTests
{
    private readonly DeckService _service = new DeckService();

    [Fact]
    public void TrimDeck_ReturnsUnchanged_WhenDeckDividesEvenly()
    {
        var deck = _service.BuildDeck(); // 52 cards
        var result = _service.TrimDeck(deck, 4); // 52 / 4 = 13, no remainder

        Assert.Equal(52, result.Count);
    }

    [Fact]
    public void TrimDeck_RemovesLowestRank_UntilEvenlyDivisible()
    {
        var deck = _service.BuildDeck(); // 52 cards
        var result = _service.TrimDeck(deck, 3); // removes 2s → 48 / 3 = 16

        Assert.Equal(48, result.Count);
        Assert.DoesNotContain(result, c => c.Rank == Rank.Two);
    }

    [Fact]
    public void TrimDeck_RemovesMultipleRanks_WhenNeeded()
    {
        var deck = _service.BuildDeck(); // 52 cards
        var result = _service.TrimDeck(deck, 5); // removes 2s, 3s, 4s → 40 / 5 = 8

        Assert.Equal(40, result.Count);
        Assert.DoesNotContain(result, c => c.Rank == Rank.Two);
        Assert.DoesNotContain(result, c => c.Rank == Rank.Three);
        Assert.DoesNotContain(result, c => c.Rank == Rank.Four);
    }

    [Fact]
    public void TrimDeck_RemovesAllSuitsOfRank_NotJustOne()
    {
        var deck = _service.BuildDeck();
        var result = _service.TrimDeck(deck, 3); // removes all four 2s

        var twos = result.Where(c => c.Rank == Rank.Two).ToList();
        Assert.Empty(twos);
    }
}
