using DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gridiron.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
    private readonly GridironDbContext _dbContext;

    public HealthController(GridironDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get()
    {
        var dbHealthy = false;
        try
        {
            dbHealthy = await _dbContext.Database.CanConnectAsync();
        }
        catch
        {
            // DB connection failed
        }

        var response = new
        {
            status = dbHealthy ? "healthy" : "degraded",
            timestamp = DateTime.UtcNow,
            checks = new
            {
                database = dbHealthy ? "connected" : "disconnected"
            }
        };

        return dbHealthy ? Ok(response) : StatusCode(503, response);
    }
}
