using WhistOnline.API.DTOs;
using WhistOnline.API.Models;

namespace WhistOnline.API.Services;

public class ScoreBoardService
{
    public List<ScoreboardEntryDto> GetScoreList(Game game)
    {
        return game.Players
            .OrderByDescending(p => p.Score)
            .Select(p => new ScoreboardEntryDto { Name = p.Name, Score  = p.Score, SeatIndex = p.SeatIndex })
            .ToList();
    }
}