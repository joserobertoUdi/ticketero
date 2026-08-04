using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/time")]
public class TimeController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetServerTime()
    {
        var now = DateTime.UtcNow;
        return Ok(new
        {
            serverTime = now.ToString("o"),
            serverTimeUtcTicks = now.Ticks,
            unixTimestampSeconds = (long)(now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
            timezone = TimeZoneInfo.Local.Id,
            timezoneUtcOffset = TimeZoneInfo.Local.BaseUtcOffset.ToString(),
            isUtc = TimeZoneInfo.Local.BaseUtcOffset == TimeSpan.Zero
        });
    }
}
