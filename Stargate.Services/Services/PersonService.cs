using Microsoft.Extensions.Logging;
using StargateAPI.Domain.Interfaces;
using StargateAPI.Domain.Models;

namespace StargateAPI.Services.Services;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<PersonService> _logger;

    public PersonService(IPersonRepository personRepository, ILogger<PersonService> logger)
    {
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Person?> GetPersonByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting person by name: {Name}", name);
        
        var person = await _personRepository.GetByNameAsync(name, cancellationToken);
        
        if (person == null)
        {
            _logger.LogWarning("Person not found with name: {Name}", name);
        }
        else
        {
            _logger.LogInformation("Successfully retrieved person: {Name} (Id: {PersonId})", name, person.Id);
        }
        
        return person;
    }

    public async Task<IEnumerable<Person>> GetAllPeopleAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all people");
        
        var people = await _personRepository.GetAllAsync(cancellationToken);
        
        _logger.LogInformation("Retrieved {Count} people", people.Count());
        
        return people;
    }

    public async Task<Person> CreatePersonAsync(string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating person with name: {Name}", name);
        
        // Business Rule: Person is uniquely identified by Name
        var existingPerson = await _personRepository.GetByNameAsync(name, cancellationToken);
        if (existingPerson != null)
        {
            _logger.LogWarning("Attempt to create duplicate person with name: {Name}", name);
            throw new InvalidOperationException($"A person with the name '{name}' already exists.");
        }
        
        var person = new Person
        {
            Name = name
        };
        
        var createdPerson = await _personRepository.CreateAsync(person, cancellationToken);
        
        _logger.LogInformation("Successfully created person: {Name} (Id: {PersonId})", name, createdPerson.Id);
        
        return createdPerson;
    }

    public async Task<Person> UpdatePersonAsync(string name, string newName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating person: {Name} to {NewName}", name, newName);
        
        var person = await _personRepository.GetByNameAsync(name, cancellationToken);
        if (person == null)
        {
            _logger.LogWarning("Person not found for update: {Name}", name);
            throw new KeyNotFoundException($"Person with name '{name}' not found.");
        }
        
        // Business Rule: Person is uniquely identified by Name
        if (name != newName)
        {
            var existingPerson = await _personRepository.GetByNameAsync(newName, cancellationToken);
            if (existingPerson != null)
            {
                _logger.LogWarning("Attempt to update person to duplicate name: {Name} -> {NewName}", name, newName);
                throw new InvalidOperationException($"A person with the name '{newName}' already exists.");
            }
        }
        
        person.Name = newName;
        var updatedPerson = await _personRepository.UpdateAsync(person, cancellationToken);
        
        _logger.LogInformation("Successfully updated person: {Name} to {NewName} (Id: {PersonId})", name, newName, updatedPerson.Id);
        
        return updatedPerson;
    }
}

