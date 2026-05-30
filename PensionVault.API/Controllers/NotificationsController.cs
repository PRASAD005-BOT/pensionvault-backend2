using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PensionVault.Application.Services;
using PensionVault.Domain.Enums;
using System.Security.Claims;

namespace PensionVault.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly IAppDbContext _context;
    public NotificationsController(IAppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .Select(n => new
            {
                n.NotificationId, n.Message,
                Category = n.Category.ToString(),
                Status = n.Status.ToString(),
                n.CreatedDate
            })
            .ToListAsync();
        return Ok(notifications);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);
        if (notification == null) return NotFound();
        notification.Status = NotificationStatus.Read;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);
        if (notification == null) return NotFound();
        notification.Status = NotificationStatus.Dismissed;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
