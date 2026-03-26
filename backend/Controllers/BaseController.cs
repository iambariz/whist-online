using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.Models;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

public class BaseController : ControllerBase
{
    protected readonly PlayerService _playerService;

    public BaseController(PlayerService playerService)
    {
        _playerService = playerService;
    }

    protected Player? GetCurrentPlayer() => _playerService.GetPlayerFromToken(User);
}
