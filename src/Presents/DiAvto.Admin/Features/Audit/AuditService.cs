using DiAvto.Admin.Data;
using DiAvto.Domain;
using Microsoft.EntityFrameworkCore;

namespace DiAvto.Admin.Features.Audit;

public sealed class AuditService
{
    private readonly DiAvtoDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        DiAvtoDbContext db,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(string action, string? details = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                UserName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "system",
                Action = action,
                Details = details
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось записать действие в журнал: {Action}", action);
        }
    }

    public async Task<List<AdminAuditLog>> GetRecentAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        return await _db.AdminAuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(cancellationToken);
    }
}

