using Microsoft.Extensions.Logging;
using StargateAPI.Domain.Interfaces;
using StargateAPI.Domain.Models;

namespace StargateAPI.Services.Services;

public class AstronautDutyService : IAstronautDutyService
{
    private readonly IPersonRepository _personRepository;
    private readonly IAstronautDutyRepository _astronautDutyRepository;
    private readonly IAstronautDetailRepository _astronautDetailRepository;
    private readonly ILogger<AstronautDutyService> _logger;

    public AstronautDutyService(
        IPersonRepository personRepository,
        IAstronautDutyRepository astronautDutyRepository,
        IAstronautDetailRepository astronautDetailRepository,
        ILogger<AstronautDutyService> logger)
    {
        _personRepository = personRepository;
        _astronautDutyRepository = astronautDutyRepository;
        _astronautDetailRepository = astronautDetailRepository;
        _logger = logger;
    }

    public async Task<(Person? Person, IEnumerable<AstronautDuty> Duties)> GetAstronautDutiesByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting astronaut duties for person: {Name}", name);
        
        var person = await _personRepository.GetByNameAsync(name, cancellationToken);
        if (person == null)
        {
            _logger.LogWarning("Person not found: {Name}", name);
            return (null, Enumerable.Empty<AstronautDuty>());
        }
        
        var duties = await _astronautDutyRepository.GetByPersonIdAsync(person.Id, cancellationToken);
        
        _logger.LogInformation("Retrieved {Count} duties for person: {Name}", duties.Count(), name);
        
        return (person, duties);
    }

    public async Task<AstronautDuty> CreateAstronautDutyAsync(string name, string rank, string dutyTitle, DateTime dutyStartDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating astronaut duty for person: {Name}, Rank: {Rank}, Title: {Title}, StartDate: {StartDate}",
            name, rank, dutyTitle, dutyStartDate);
        
        // Business Rule: Person must exist
        var person = await _personRepository.GetByNameAsync(name, cancellationToken);
        if (person == null)
        {
            _logger.LogError("Cannot create astronaut duty: Person not found: {Name}", name);
            throw new KeyNotFoundException($"Person with name '{name}' not found.");
        }
        
        // Business Rule: Verify no duplicate duty (same title and start date)
        var existingDuties = await _astronautDutyRepository.GetByPersonIdAsync(person.Id, cancellationToken);
        var duplicateDuty = existingDuties.FirstOrDefault(d => d.DutyTitle == dutyTitle && d.DutyStartDate.Date == dutyStartDate.Date);
        if (duplicateDuty != null)
        {
            _logger.LogWarning("Attempt to create duplicate astronaut duty: Person: {Name}, Title: {Title}, StartDate: {StartDate}", name, dutyTitle, dutyStartDate);
            throw new InvalidOperationException($"An astronaut duty with title '{dutyTitle}' and start date '{dutyStartDate:yyyy-MM-dd}' already exists for this person.");
        }
        
        // Business Rule: Get current duty and set its end date
        var currentDuty = await _astronautDutyRepository.GetCurrentDutyByPersonIdAsync(person.Id, cancellationToken);
        if (currentDuty != null)
        {
            // Business Rule: Previous Duty EndDate = day before new Duty StartDate
            currentDuty.DutyEndDate = dutyStartDate.AddDays(-1).Date;
            await _astronautDutyRepository.UpdateAsync(currentDuty, cancellationToken);
            _logger.LogInformation(
                "Updated previous duty end date to {EndDate} for person: {Name}",
                currentDuty.DutyEndDate, name);
        }
        
        // Create or update AstronautDetail
        var astronautDetail = await _astronautDetailRepository.GetByPersonIdAsync(person.Id, cancellationToken);
        
        if (astronautDetail == null)
        {
            // Business Rule: Person without astronaut assignment has no Astronaut records (creating first one)
            astronautDetail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = rank,
                CurrentDutyTitle = dutyTitle,
                CareerStartDate = dutyStartDate.Date
            };
            
            // Business Rule: If DutyTitle is 'RETIRED', CareerEndDate = day before Retired Duty StartDate
            if (dutyTitle == "RETIRED")
            {
                astronautDetail.CareerEndDate = dutyStartDate.AddDays(-1).Date;
            }
            
            await _astronautDetailRepository.CreateAsync(astronautDetail, cancellationToken);
            _logger.LogInformation("Created new astronaut detail for person: {Name}", name);
        }
        else
        {
            astronautDetail.CurrentRank = rank;
            astronautDetail.CurrentDutyTitle = dutyTitle;
            
            // Business Rule: If DutyTitle is 'RETIRED', CareerEndDate = day before Retired Duty StartDate
            if (dutyTitle == "RETIRED")
            {
                astronautDetail.CareerEndDate = dutyStartDate.AddDays(-1).Date;
            }
            
            await _astronautDetailRepository.UpdateAsync(astronautDetail, cancellationToken);
            _logger.LogInformation("Updated astronaut detail for person: {Name}", name);
        }
        
        // Business Rule: Create new duty (Current Duty has no DutyEndDate)
        var newAstronautDuty = new AstronautDuty
        {
            PersonId = person.Id,
            Rank = rank,
            DutyTitle = dutyTitle,
            DutyStartDate = dutyStartDate.Date,
            DutyEndDate = null // Business Rule: Current Duty has no DutyEndDate
        };
        
        var createdDuty = await _astronautDutyRepository.CreateAsync(newAstronautDuty, cancellationToken);
        
        _logger.LogInformation(
            "Successfully created astronaut duty: Person: {Name}, DutyId: {DutyId}, Title: {Title}",
            name, createdDuty.Id, dutyTitle);
        
        return createdDuty;
    }
}

