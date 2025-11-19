using Microsoft.EntityFrameworkCore;
using TrafficTrack.Core.Interfaces;
using TrafficTrack.Core.Models;

namespace TrafficTrack.Infrastructure.Data;

public class TrafficRepository : ITrafficRepository
{
    private readonly TrafficDbContext _context;

    public TrafficRepository(TrafficDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveTrafficIncidentsAsync(IEnumerable<TrafficIncident> incidents, CancellationToken cancellationToken = default)
    {
        await _context.TrafficIncidents.AddRangeAsync(incidents, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveTrafficFlowsAsync(IEnumerable<TrafficFlow> flows, CancellationToken cancellationToken = default)
    {
        await _context.TrafficFlows.AddRangeAsync(flows, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<TrafficIncident>> GetTrafficIncidentsAsync(DateTime? from = null, DateTime? to = null, string? area = null, string? severity = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TrafficIncidents.AsQueryable();

        if (from.HasValue)
            query = query.Where(i => i.RecordedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(i => i.RecordedAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(area))
            query = query.Where(i => i.Area == area);

        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(i => i.Severity == severity);

        return await query.OrderByDescending(i => i.RecordedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TrafficFlow>> GetTrafficFlowsAsync(DateTime? from = null, DateTime? to = null, string? area = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TrafficFlows.AsQueryable();

        if (from.HasValue)
            query = query.Where(f => f.RecordedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(f => f.RecordedAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(area))
            query = query.Where(f => f.Area == area);

        return await query.OrderByDescending(f => f.RecordedAt).ToListAsync(cancellationToken);
    }
}
