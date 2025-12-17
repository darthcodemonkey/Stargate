using Microsoft.EntityFrameworkCore;
using StargateAPI.Domain.Data;
using StargateAPI.Domain.Interfaces;
using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Repositories;

public class AstronautDetailRepository : IAstronautDetailRepository
{
    private readonly StargateContext _context;

    public AstronautDetailRepository(StargateContext context)
    {
        _context = context;
    }

    public async Task<AstronautDetail?> GetByPersonIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        return await _context.AstronautDetails
            .FirstOrDefaultAsync(d => d.PersonId == personId, cancellationToken);
    }

    public async Task<AstronautDetail> CreateAsync(AstronautDetail detail, CancellationToken cancellationToken = default)
    {
        await _context.AstronautDetails.AddAsync(detail, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return detail;
    }

    public async Task<AstronautDetail> UpdateAsync(AstronautDetail detail, CancellationToken cancellationToken = default)
    {
        _context.AstronautDetails.Update(detail);
        await _context.SaveChangesAsync(cancellationToken);
        return detail;
    }
}

