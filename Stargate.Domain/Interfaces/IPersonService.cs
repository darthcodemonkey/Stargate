using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Interfaces;

public interface IPersonService
{
    Task<Person?> GetPersonByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Person>> GetAllPeopleAsync(CancellationToken cancellationToken = default);
    Task<Person> CreatePersonAsync(string name, CancellationToken cancellationToken = default);
    Task<Person> UpdatePersonAsync(string name, string newName, CancellationToken cancellationToken = default);
}

