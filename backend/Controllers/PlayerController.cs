using Microsoft.AspNetCore.Mvc;

namespace WhistOnline.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    [HttpGet]
    public IActionResult GetLobbies()
    {
        return Ok(new[] {
            new { Id = Guid.NewGuid(), Name = "Test Lobby", PlayerCount = 2 }
        });
    }
}