namespace WhistOnline.Tests.Integration;

using Microsoft.EntityFrameworkCore;
using WhistOnline.API.Actions;
using WhistOnline.API.Data;
using WhistOnline.API.DTOs;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;
using WhistOnline.API.Services;

public class FullGameCycleTests
{
    // -------------------------------------------------------------------------
    // Infrastructure
    // -------------------------------------------------------------------------

    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private GameService CreateGameService(AppDbContext db) =>
        new GameService(new GameRepository(db), new DeckService());

    private (Game game, Player p0, Player p1, Player p2) SetupGame(AppDbContext db)
    {
        var p0 = new Player { Id = Guid.NewGuid(), Name = "Alice", SeatIndex = 0 };
        var p1 = new Player { Id = Guid.NewGuid(), Name = "Bob",   SeatIndex = 1 };
        var p2 = new Player { Id = Guid.NewGuid(), Name = "Carol", SeatIndex = 2 };
        var game = new Game { Players = [p0, p1, p2] };
        db.Games.Add(game);
        db.SaveChanges();
        return (game, p0, p1, p2);
    }

    private bool Bid(Game game, Player player, int amount, AppDbContext db)
    {
        var action = new BidAction(new GameRules(), new BidRepository(db), amount);
        return action.Execute(game, player);
    }

    private bool Play(Game game, Player player, Suit suit, Rank rank, AppDbContext db, GameService gameService)
    {
        var action = new PlayCardAction(
            new GameRules(),
            new PlayCardDto { Suit = suit, Rank = rank },
            new TrickService(),
            new ScoringService(),
            gameService);
        return action.Execute(game, player);
    }

    private void BidRound(Game game, (Player player, int amount)[] bids, AppDbContext db)
    {
        foreach (var (player, amount) in bids)
            Bid(game, player, amount, db);
    }

    private void PlayRound(Game game, List<(Player player, Suit suit, Rank rank)[]> tricks, AppDbContext db, GameService gs)
    {
        foreach (var trick in tricks)
            foreach (var (player, suit, rank) in trick)
                Play(game, player, suit, rank, db, gs);
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Fact]
    public void Round1_SingleTrick_ScoresAndAdvancesToRound2()
    {
        var db = CreateDb();
        var gs = CreateGameService(db);
        var (dbGame, p0, p1, p2) = SetupGame(db);

        // StartGame: round 1, 1 card each, trump=Clubs, dealer=p0, p1 leads bidding
        var game = gs.StartGame(dbGame.Id, p0.Id)!;

        // p1 wins with trump (Clubs Ace), others have Hearts
        p1.Hand = [new Card { Suit = Suit.Clubs, Rank = Rank.Ace }];
        p2.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.King }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Two }];

        // Bidding: p1→p2→p0(dealer). p0 cannot bid 0 (1+0+0=1=cardsDealt)
        Assert.True(Bid(game, p1, 1, db));
        Assert.True(Bid(game, p2, 0, db));
        Assert.True(Bid(game, p0, 1, db));
        Assert.Equal(GameStatus.Playing, game.Status);

        // Playing: p1 leads
        Assert.True(Play(game, p1, Suit.Clubs, Rank.Ace, db, gs));
        Assert.True(Play(game, p2, Suit.Hearts, Rank.King, db, gs));
        Assert.True(Play(game, p0, Suit.Hearts, Rank.Two, db, gs));

        var trick = game.Rounds.First().Tricks.First();
        Assert.Equal(p1.Id, trick.WinnerPlayerId);

        Assert.Equal(11, p1.Score);  // hit bid 1 → +11
        Assert.Equal(10, p2.Score);  // hit bid 0 → +10
        Assert.Equal(-1, p0.Score);  // missed bid 1, won 0 → -1

        Assert.Equal(2, game.CurrentRound);
        Assert.Equal(GameStatus.Bidding, game.Status);
        Assert.Equal(Suit.Diamonds, game.TrumpSuit);
        Assert.Equal(1, game.DealerIndex);
        Assert.Equal(2, game.CurrentPlayerIndex);
        Assert.All(game.Players, p => Assert.Equal(2, p.Hand.Count));
    }

    [Fact]
    public void Round2_TwoTricks_CumulativeScoring_AdvancesToRound3()
    {
        var db = CreateDb();
        var gs = CreateGameService(db);
        var (dbGame, p0, p1, p2) = SetupGame(db);

        var game = gs.StartGame(dbGame.Id, p0.Id)!;

        // --- Round 1 (same as above) ---
        p1.Hand = [new Card { Suit = Suit.Clubs, Rank = Rank.Ace }];
        p2.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.King }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Two }];

        Bid(game, p1, 1, db);
        Bid(game, p2, 0, db);
        Bid(game, p0, 1, db);
        Play(game, p1, Suit.Clubs, Rank.Ace, db, gs);
        Play(game, p2, Suit.Hearts, Rank.King, db, gs);
        Play(game, p0, Suit.Hearts, Rank.Two, db, gs);

        // --- Round 2: trump=Diamonds, dealer=p1, bidding starts at p2 ---
        // Override hands: p2 wins trick 1 (Hearts Ace), p1 wins trick 2 (Clubs Ace)
        p2.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Ace }, new Card { Suit = Suit.Clubs, Rank = Rank.King }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.King }, new Card { Suit = Suit.Clubs, Rank = Rank.Two }];
        p1.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Queen }, new Card { Suit = Suit.Clubs, Rank = Rank.Ace }];

        // Bidding: p2→p0→p1(dealer). p1 can't bid 1 (1+0+1=2=cardsDealt)
        Assert.True(Bid(game, p2, 1, db));
        Assert.True(Bid(game, p0, 0, db));
        Assert.True(Bid(game, p1, 0, db));
        Assert.Equal(GameStatus.Playing, game.Status);

        // Trick 1: p2 leads Hearts Ace (all must follow Hearts)
        Assert.True(Play(game, p2, Suit.Hearts, Rank.Ace,   db, gs));
        Assert.True(Play(game, p0, Suit.Hearts, Rank.King,  db, gs));
        Assert.True(Play(game, p1, Suit.Hearts, Rank.Queen, db, gs));

        var round2 = game.Rounds.Last();
        Assert.Equal(p2.Id, round2.Tricks[0].WinnerPlayerId);
        Assert.Equal(2, round2.Tricks.Count); // trick 2 was created

        // Trick 2: p2 leads Clubs King (all must follow Clubs)
        Assert.True(Play(game, p2, Suit.Clubs, Rank.King, db, gs));
        Assert.True(Play(game, p0, Suit.Clubs, Rank.Two,  db, gs));
        Assert.True(Play(game, p1, Suit.Clubs, Rank.Ace,  db, gs));

        Assert.Equal(p1.Id, round2.Tricks[1].WinnerPlayerId);

        // Cumulative scores after round 2
        // p0: round1=-1, round2 hit(0=0)=+10 → total=9
        // p1: round1=+11, round2 miss(0≠1)=-1 → total=10
        // p2: round1=+10, round2 hit(1=1)=+11 → total=21
        Assert.Equal(9,  p0.Score);
        Assert.Equal(10, p1.Score);
        Assert.Equal(21, p2.Score);

        Assert.Equal(3, game.CurrentRound);
        Assert.Equal(GameStatus.Bidding, game.Status);
        Assert.Equal(Suit.Hearts, game.TrumpSuit);
        Assert.Equal(2, game.DealerIndex);
        Assert.Equal(0, game.CurrentPlayerIndex);
        Assert.All(game.Players, p => Assert.Equal(3, p.Hand.Count));
    }

    [Fact]
    public void LastRound_SetsGameToFinished()
    {
        var db = CreateDb();
        var gs = CreateGameService(db);
        var (dbGame, p0, p1, p2) = SetupGame(db);

        var game = gs.StartGame(dbGame.Id, p0.Id)!;
        game.TotalRounds = 1; // make this the final round

        p1.Hand = [new Card { Suit = Suit.Clubs, Rank = Rank.Ace }];
        p2.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.King }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Two }];

        Bid(game, p1, 1, db);
        Bid(game, p2, 0, db);
        Bid(game, p0, 1, db);
        Play(game, p1, Suit.Clubs, Rank.Ace,   db, gs);
        Play(game, p2, Suit.Hearts, Rank.King,  db, gs);
        Play(game, p0, Suit.Hearts, Rank.Two,   db, gs);

        Assert.Equal(GameStatus.Finished, game.Status);
    }

    [Fact]
    public void NoTrumpRound_HighestLeadSuitWins()
    {
        var db = CreateDb();
        var gs = CreateGameService(db);
        var (dbGame, p0, p1, p2) = SetupGame(db);

        var game = gs.StartGame(dbGame.Id, p0.Id)!;
        game.TrumpSuit = null; // no trump

        // p1 leads Hearts King; p2 sluffs Diamonds Ace (not lead suit, no trump); p0 follows Hearts Two
        p1.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.King }];
        p2.Hand = [new Card { Suit = Suit.Diamonds, Rank = Rank.Ace }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Two }];

        Bid(game, p1, 1, db);
        Bid(game, p2, 0, db);
        Bid(game, p0, 1, db);
        Play(game, p1, Suit.Hearts, Rank.King,    db, gs);
        Play(game, p2, Suit.Diamonds, Rank.Ace,   db, gs);
        Play(game, p0, Suit.Hearts, Rank.Two,     db, gs);

        var trick = game.Rounds.First().Tricks.First();
        Assert.Equal(p1.Id, trick.WinnerPlayerId); // Hearts King wins, no trump
    }

    [Fact]
    public void TrickWinner_LeadsNextTrick()
    {
        var db = CreateDb();
        var gs = CreateGameService(db);
        var (dbGame, p0, p1, p2) = SetupGame(db);

        var game = gs.StartGame(dbGame.Id, p0.Id)!;

        // Complete round 1, p1 wins
        p1.Hand = [new Card { Suit = Suit.Clubs, Rank = Rank.Ace }];
        p2.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.King }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Two }];

        Bid(game, p1, 1, db);
        Bid(game, p2, 0, db);
        Bid(game, p0, 1, db);
        Play(game, p1, Suit.Clubs,  Rank.Ace,  db, gs);
        Play(game, p2, Suit.Hearts, Rank.King, db, gs);
        Play(game, p0, Suit.Hearts, Rank.Two,  db, gs);

        // Round 2: override hands, p1 wins trick 1
        p2.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Two }, new Card { Suit = Suit.Clubs, Rank = Rank.Two }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Three }, new Card { Suit = Suit.Clubs, Rank = Rank.Three }];
        p1.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Ace }, new Card { Suit = Suit.Clubs, Rank = Rank.Four }];

        Bid(game, p2, 0, db);
        Bid(game, p0, 0, db);
        Bid(game, p1, 0, db); // dealer p1 can't bid 1 (0+0+1=1≠2), bids 0

        // Trick 1: p2 leads Hearts (p2 is CurrentPlayerIndex=2 after round 1)
        Play(game, p2, Suit.Hearts, Rank.Two,   db, gs);
        Play(game, p0, Suit.Hearts, Rank.Three, db, gs);
        Play(game, p1, Suit.Hearts, Rank.Ace,   db, gs); // p1 wins

        var round2 = game.Rounds.Last();
        Assert.Equal(p1.Id, round2.Tricks[0].WinnerPlayerId);

        // p1 must lead trick 2 — CurrentPlayerIndex should be p1's seat
        Assert.Equal(p1.SeatIndex, game.CurrentPlayerIndex);
    }

    [Fact]
    public void FullGame_AllTrumpSuits_IncludingNoTrump_SetsFinished()
    {
        // 5 rounds: Clubs → Diamonds → Hearts → Spades → null (no trump) → Finished
        // Each round N, one player sweeps all N tricks with trump (or lead suit in no-trump round)
        // Round 1: p1 sweeps with Clubs (trump)
        // Round 2: p2 sweeps with Diamonds (trump)
        // Round 3: p0 sweeps with Hearts (trump)
        // Round 4: p1 sweeps with Spades (trump)
        // Round 5: p2 sweeps with Hearts (lead suit, no trump)

        var db = CreateDb();
        var gs = CreateGameService(db);
        var (dbGame, p0, p1, p2) = SetupGame(db);

        var game = gs.StartGame(dbGame.Id, p0.Id)!;
        game.TotalRounds = 5;

        // ── Round 1: Trump=Clubs, 1 card, dealer=p0, p1 leads bidding ──────────
        p1.Hand = [new Card { Suit = Suit.Clubs,  Rank = Rank.Ace }];
        p2.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.King }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Two }];

        // p0 (dealer) can't bid 0 (1+0+0=1=cardsDealt), bids 1
        BidRound(game, [(p1, 1), (p2, 0), (p0, 1)], db);
        Assert.Equal(Suit.Clubs, game.TrumpSuit);

        PlayRound(game, [
            [(p1, Suit.Clubs, Rank.Ace), (p2, Suit.Hearts, Rank.King), (p0, Suit.Hearts, Rank.Two)]
        ], db, gs);

        Assert.Equal(p1.Id, game.Rounds[0].Tricks[0].WinnerPlayerId);
        Assert.Equal(11, p1.Score); Assert.Equal(10, p2.Score); Assert.Equal(-1, p0.Score);
        Assert.Equal(Suit.Diamonds, game.TrumpSuit);

        // ── Round 2: Trump=Diamonds, 2 cards, dealer=p1, p2 leads bidding ──────
        p2.Hand = [new Card { Suit = Suit.Diamonds, Rank = Rank.Ace  }, new Card { Suit = Suit.Diamonds, Rank = Rank.King }];
        p0.Hand = [new Card { Suit = Suit.Hearts,   Rank = Rank.Ace  }, new Card { Suit = Suit.Hearts,   Rank = Rank.King }];
        p1.Hand = [new Card { Suit = Suit.Spades,   Rank = Rank.Ace  }, new Card { Suit = Suit.Spades,   Rank = Rank.King }];

        // p1 (dealer) can't bid 0 (2+0+0=2=cardsDealt), bids 1
        BidRound(game, [(p2, 2), (p0, 0), (p1, 1)], db);
        Assert.Equal(Suit.Diamonds, game.TrumpSuit);

        PlayRound(game, [
            [(p2, Suit.Diamonds, Rank.Ace),  (p0, Suit.Hearts, Rank.Ace),  (p1, Suit.Spades, Rank.Ace)],
            [(p2, Suit.Diamonds, Rank.King), (p0, Suit.Hearts, Rank.King), (p1, Suit.Spades, Rank.King)]
        ], db, gs);

        Assert.Equal(p2.Id, game.Rounds[1].Tricks[0].WinnerPlayerId);
        Assert.Equal(p2.Id, game.Rounds[1].Tricks[1].WinnerPlayerId);
        Assert.Equal(9, p0.Score); Assert.Equal(10, p1.Score); Assert.Equal(22, p2.Score);
        Assert.Equal(Suit.Hearts, game.TrumpSuit);

        // ── Round 3: Trump=Hearts, 3 cards, dealer=p2, p0 leads bidding ─────────
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Ace   }, new Card { Suit = Suit.Hearts, Rank = Rank.King  }, new Card { Suit = Suit.Hearts, Rank = Rank.Queen }];
        p1.Hand = [new Card { Suit = Suit.Clubs,  Rank = Rank.Ace   }, new Card { Suit = Suit.Clubs,  Rank = Rank.King  }, new Card { Suit = Suit.Clubs,  Rank = Rank.Queen }];
        p2.Hand = [new Card { Suit = Suit.Spades, Rank = Rank.Ace   }, new Card { Suit = Suit.Spades, Rank = Rank.King  }, new Card { Suit = Suit.Spades, Rank = Rank.Queen }];

        // p2 (dealer) can't bid 0 (3+0+0=3=cardsDealt), bids 1
        BidRound(game, [(p0, 3), (p1, 0), (p2, 1)], db);
        Assert.Equal(Suit.Hearts, game.TrumpSuit);

        PlayRound(game, [
            [(p0, Suit.Hearts, Rank.Ace),   (p1, Suit.Clubs, Rank.Ace),   (p2, Suit.Spades, Rank.Ace)],
            [(p0, Suit.Hearts, Rank.King),  (p1, Suit.Clubs, Rank.King),  (p2, Suit.Spades, Rank.King)],
            [(p0, Suit.Hearts, Rank.Queen), (p1, Suit.Clubs, Rank.Queen), (p2, Suit.Spades, Rank.Queen)]
        ], db, gs);

        Assert.All(game.Rounds[2].Tricks, t => Assert.Equal(p0.Id, t.WinnerPlayerId));
        Assert.Equal(22, p0.Score); Assert.Equal(20, p1.Score); Assert.Equal(21, p2.Score);
        Assert.Equal(Suit.Spades, game.TrumpSuit);

        // ── Round 4: Trump=Spades, 4 cards, dealer=p0, p1 leads bidding ─────────
        p1.Hand = [new Card { Suit = Suit.Spades, Rank = Rank.Ace   }, new Card { Suit = Suit.Spades, Rank = Rank.King  }, new Card { Suit = Suit.Spades, Rank = Rank.Queen }, new Card { Suit = Suit.Spades, Rank = Rank.Jack }];
        p2.Hand = [new Card { Suit = Suit.Clubs,  Rank = Rank.Ace   }, new Card { Suit = Suit.Clubs,  Rank = Rank.King  }, new Card { Suit = Suit.Clubs,  Rank = Rank.Queen }, new Card { Suit = Suit.Clubs,  Rank = Rank.Jack }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Ace   }, new Card { Suit = Suit.Hearts, Rank = Rank.King  }, new Card { Suit = Suit.Hearts, Rank = Rank.Queen }, new Card { Suit = Suit.Hearts, Rank = Rank.Jack }];

        // p0 (dealer) can't bid 0 (4+0+0=4=cardsDealt), bids 1
        BidRound(game, [(p1, 4), (p2, 0), (p0, 1)], db);
        Assert.Equal(Suit.Spades, game.TrumpSuit);

        PlayRound(game, [
            [(p1, Suit.Spades, Rank.Ace),   (p2, Suit.Clubs, Rank.Ace),   (p0, Suit.Hearts, Rank.Ace)],
            [(p1, Suit.Spades, Rank.King),  (p2, Suit.Clubs, Rank.King),  (p0, Suit.Hearts, Rank.King)],
            [(p1, Suit.Spades, Rank.Queen), (p2, Suit.Clubs, Rank.Queen), (p0, Suit.Hearts, Rank.Queen)],
            [(p1, Suit.Spades, Rank.Jack),  (p2, Suit.Clubs, Rank.Jack),  (p0, Suit.Hearts, Rank.Jack)]
        ], db, gs);

        Assert.All(game.Rounds[3].Tricks, t => Assert.Equal(p1.Id, t.WinnerPlayerId));
        Assert.Equal(21, p0.Score); Assert.Equal(34, p1.Score); Assert.Equal(31, p2.Score);
        Assert.Null(game.TrumpSuit);

        // ── Round 5: No Trump, 5 cards, dealer=p1, p2 leads bidding ─────────────
        // p2 sweeps all 5 tricks with highest Hearts each time (no trump, p0 must follow, p1 sluffs)
        p2.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Ace }, new Card { Suit = Suit.Hearts, Rank = Rank.King  }, new Card { Suit = Suit.Hearts, Rank = Rank.Queen }, new Card { Suit = Suit.Hearts, Rank = Rank.Jack }, new Card { Suit = Suit.Hearts, Rank = Rank.Ten }];
        p0.Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Two }, new Card { Suit = Suit.Hearts, Rank = Rank.Three }, new Card { Suit = Suit.Hearts, Rank = Rank.Four  }, new Card { Suit = Suit.Hearts, Rank = Rank.Five }, new Card { Suit = Suit.Hearts, Rank = Rank.Six }];
        p1.Hand = [new Card { Suit = Suit.Spades, Rank = Rank.Ace }, new Card { Suit = Suit.Spades, Rank = Rank.King  }, new Card { Suit = Suit.Spades, Rank = Rank.Queen }, new Card { Suit = Suit.Spades, Rank = Rank.Jack }, new Card { Suit = Suit.Spades, Rank = Rank.Ten }];

        // p1 (dealer) can't bid 0 (5+0+0=5=cardsDealt), bids 1
        BidRound(game, [(p2, 5), (p0, 0), (p1, 1)], db);
        Assert.Null(game.TrumpSuit);

        PlayRound(game, [
            [(p2, Suit.Hearts, Rank.Ace),   (p0, Suit.Hearts, Rank.Two),   (p1, Suit.Spades, Rank.Ace)],
            [(p2, Suit.Hearts, Rank.King),  (p0, Suit.Hearts, Rank.Three), (p1, Suit.Spades, Rank.King)],
            [(p2, Suit.Hearts, Rank.Queen), (p0, Suit.Hearts, Rank.Four),  (p1, Suit.Spades, Rank.Queen)],
            [(p2, Suit.Hearts, Rank.Jack),  (p0, Suit.Hearts, Rank.Five),  (p1, Suit.Spades, Rank.Jack)],
            [(p2, Suit.Hearts, Rank.Ten),   (p0, Suit.Hearts, Rank.Six),   (p1, Suit.Spades, Rank.Ten)]
        ], db, gs);

        Assert.All(game.Rounds[4].Tricks, t => Assert.Equal(p2.Id, t.WinnerPlayerId));

        // Final scores
        Assert.Equal(31, p0.Score);
        Assert.Equal(33, p1.Score);
        Assert.Equal(46, p2.Score);
        Assert.Equal(GameStatus.Finished, game.Status);
    }
}
