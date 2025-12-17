using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Interfaces;

public interface IPersonRepository
{
    Task<Person?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default);
    Task<Person> UpdateAsync(Person person, CancellationToken cancellationToken = default);
}

