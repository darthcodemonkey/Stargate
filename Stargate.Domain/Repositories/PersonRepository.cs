using Microsoft.EntityFrameworkCore;
using StargateAPI.Domain.Data;
using StargateAPI.Domain.Interfaces;
using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly StargateContext _context;

    public PersonRepository(StargateContext context)
    {
        _context = context;
    }

    public async Task<Person?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.People
            .Include(p => p.AstronautDetail)
            .Include(p => p.AstronautDuties)
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.People
            .Include(p => p.AstronautDetail)
            .Include(p => p.AstronautDuties)
            .ToListAsync(cancellationToken);
    }

    public async Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default)
    {
        await _context.People.AddAsync(person, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return person;
    }

    public async Task<Person> UpdateAsync(Person person, CancellationToken cancellationToken = default)
    {
        _context.People.Update(person);
        await _context.SaveChangesAsync(cancellationToken);
        return person;
    }
}

