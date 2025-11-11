using EventManager.Api.Dtos;
using EventManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        // Constructor with dependency injection
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: api/Dashboard
        // Fetches dashboard data for the authenticated user
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                // Extract UserId from JWT claim
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"[DashboardController] Extracted userId from JWT: {userId}");

                // Validate userId
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("[DashboardController] UserId is null or empty");
                    return Unauthorized("User not authenticated.");
                }

                // Fetch dashboard data
                var dashboardData = await _dashboardService.GetDashboardDataAsync(userId);
                Console.WriteLine($"[DashboardController] Successfully fetched dashboard data for userId: {userId}");

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                // Log error and return 500
                Console.WriteLine($"[DashboardController] Error fetching dashboard data: {ex.Message}");
                return StatusCode(500, $"An error occurred while fetching dashboard data: {ex.Message}");
            }
        }
    }
}