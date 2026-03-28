using WhistOnline.API.DTOs;
using WhistOnline.API.Models;
using WhistOnline.API.Repositories;

namespace WhistOnline.API.Services;

public class TrickService
{
    private readonly GameRepository _gameRepository;

    public TrickService(GameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public bool PlayCard(PlayCardDto playCardDto, Guid gameId, Player player)
    {
        var game = _gameRepository.FindById(gameId);
        
        if(game is not { Status: GameStatus.Playing }) return false;
        if (!PlayerIsInTurn(game, player)) return false;
        if (!IsCardPlayedValid(playCardDto, game, player)) return false;


        return false;
        // 1. Find the card in the player's hand                                                                                                                                                                                                                                                                               
        // 2. Add it to the current trick's CardsPlayed                                                                                                                                                                                                                                                                        
        // 3. Set the trick's LeadSuit if it's the first card                                                                                                                                                                                                                                                                  
        // 4. Remove the card from the player's hand                                                                                                                                                                                                                                                                           
        // 5. Advance CurrentPlayerIndex
        // 6. Save  
    }
    

    private bool IsCardPlayedValid(PlayCardDto playCardDto, Game game, Player player)
    {
        if (!PlayerHasCard(playCardDto, player)) return false;
        var currentTrick = game.Rounds.Last().Tricks.LastOrDefault();                                                                                                                                                                                                                                                       
        if(currentTrick == null) return false;
        var card = new Card { Suit = playCardDto.Suit, Rank = playCardDto.Rank };                                                                                                                                                                                                                                           

        return IsValidPlay(card, player,currentTrick);
    }
    
    private bool PlayerHasCard(PlayCardDto playCardDto, Player player)
    {
        return player.Hand.Any(c => c.Suit == playCardDto.Suit && c.Rank == playCardDto.Rank);
    }
    
    private bool IsValidPlay(Card card, Player player, Trick currentTrick)                                                                                                                                                                                                                                              
    {                                                                                                                                                                                                                                                                                                                   
        if (currentTrick.LeadSuit == null) return true;
                                                                                                                                                                                                                                                                                                                      
        var hasLeadSuit = player.Hand.Any(c => c.Suit == currentTrick.LeadSuit);                                                                                                                                                                                                                                        
        if (!hasLeadSuit) return true;
                                                                                                                                                                                                                                                                                                                      
        return card.Suit == currentTrick.LeadSuit;
    }

    private bool PlayerIsInTurn(Game game, Player player)
    {
        return game.CurrentPlayerIndex == player.SeatIndex;
    }

    public void EvaluateTrick(Game game)
    {
        
    }

   
}
