using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Interfaces;

public interface IAstronautDutyRepository
{
    Task<IEnumerable<AstronautDuty>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<AstronautDuty?> GetCurrentDutyByPersonIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<AstronautDuty> CreateAsync(AstronautDuty duty, CancellationToken cancellationToken = default);
    Task<AstronautDuty> UpdateAsync(AstronautDuty duty, CancellationToken cancellationToken = default);
}

