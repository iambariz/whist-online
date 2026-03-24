using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhistOnline.API.DTOs;
using WhistOnline.API.Services;

namespace WhistOnline.API.Controllers;

[ApiController]                                                                                                                                                                       
[Route("api/[controller]")]                                                                                                                                                           
public class GameController : ControllerBase                                                                                                                                          
{                                                                                                                                                                                     
    private readonly GameService _gameService;            

    public GameController(GameService gameService)
    {
        _gameService = gameService;
    }

    [Authorize]
    [HttpPost("{id:guid}/start")]
    public IActionResult StartGame(Guid id)
    {                                                                                                                                                                                 
        // call service
        // return result                                                                                                                                                              
    }                                                     
}
