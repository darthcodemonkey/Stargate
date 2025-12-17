using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Interfaces;

public interface IAstronautDetailRepository
{
    Task<AstronautDetail?> GetByPersonIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<AstronautDetail> CreateAsync(AstronautDetail detail, CancellationToken cancellationToken = default);
    Task<AstronautDetail> UpdateAsync(AstronautDetail detail, CancellationToken cancellationToken = default);
}

