using Microsoft.EntityFrameworkCore;
using StargateAPI.Domain.Data;
using StargateAPI.Domain.Interfaces;
using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Repositories;

public class AstronautDutyRepository : IAstronautDutyRepository
{
    private readonly StargateContext _context;

    public AstronautDutyRepository(StargateContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AstronautDuty>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        return await _context.AstronautDuties
            .Where(d => d.PersonId == personId)
            .OrderByDescending(d => d.DutyStartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<AstronautDuty?> GetCurrentDutyByPersonIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        return await _context.AstronautDuties
            .Where(d => d.PersonId == personId && d.DutyEndDate == null)
            .OrderByDescending(d => d.DutyStartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AstronautDuty> CreateAsync(AstronautDuty duty, CancellationToken cancellationToken = default)
    {
        await _context.AstronautDuties.AddAsync(duty, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return duty;
    }

    public async Task<AstronautDuty> UpdateAsync(AstronautDuty duty, CancellationToken cancellationToken = default)
    {
        _context.AstronautDuties.Update(duty);
        await _context.SaveChangesAsync(cancellationToken);
        return duty;
    }
}

