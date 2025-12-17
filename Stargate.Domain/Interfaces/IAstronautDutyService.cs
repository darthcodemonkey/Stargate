using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Interfaces;

public interface IAstronautDutyService
{
    Task<(Person? Person, IEnumerable<AstronautDuty> Duties)> GetAstronautDutiesByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<AstronautDuty> CreateAstronautDutyAsync(string name, string rank, string dutyTitle, DateTime dutyStartDate, CancellationToken cancellationToken = default);
}

