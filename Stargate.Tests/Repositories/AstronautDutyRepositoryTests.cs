using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Domain.Data;
using StargateAPI.Domain.Models;
using StargateAPI.Domain.Repositories;

namespace StargateAPI.Tests.Repositories;

public class AstronautDutyRepositoryTests : IDisposable
{
    private readonly StargateContext _context;
    private readonly AstronautDutyRepository _repository;

    public AstronautDutyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StargateContext(options);
        _repository = new AstronautDutyRepository(_context);
    }

    [Fact]
    public async Task GetByPersonIdAsync_ReturnsDutiesOrderedByStartDateDescending()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        var duty1 = new AstronautDuty 
        { 
            PersonId = person.Id, 
            Rank = "Captain", 
            DutyTitle = "Commander", 
            DutyStartDate = DateTime.Now.AddDays(-30),
            DutyEndDate = DateTime.Now.AddDays(-10)
        };
        var duty2 = new AstronautDuty 
        { 
            PersonId = person.Id, 
            Rank = "Major", 
            DutyTitle = "Pilot", 
            DutyStartDate = DateTime.Now.AddDays(-10),
            DutyEndDate = null // Current duty
        };
        _context.AstronautDuties.AddRange(duty1, duty2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPersonIdAsync(person.Id);

        // Assert
        result.Should().HaveCount(2);
        result.First().DutyTitle.Should().Be("Pilot"); // Most recent first
        result.Last().DutyTitle.Should().Be("Commander");
    }

    [Fact]
    public async Task GetCurrentDutyByPersonIdAsync_WhenCurrentDutyExists_ReturnsDutyWithoutEndDate()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        var currentDuty = new AstronautDuty 
        { 
            PersonId = person.Id, 
            Rank = "Captain", 
            DutyTitle = "Commander", 
            DutyStartDate = DateTime.Now.AddDays(-10),
            DutyEndDate = null // Current duty
        };
        var pastDuty = new AstronautDuty 
        { 
            PersonId = person.Id, 
            Rank = "Major", 
            DutyTitle = "Pilot", 
            DutyStartDate = DateTime.Now.AddDays(-30),
            DutyEndDate = DateTime.Now.AddDays(-10)
        };
        _context.AstronautDuties.AddRange(pastDuty, currentDuty);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCurrentDutyByPersonIdAsync(person.Id);

        // Assert
        result.Should().NotBeNull();
        result!.DutyEndDate.Should().BeNull();
        result.DutyTitle.Should().Be("Commander");
    }

    [Fact]
    public async Task GetCurrentDutyByPersonIdAsync_WhenNoCurrentDuty_ReturnsNull()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCurrentDutyByPersonIdAsync(person.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_CreatesDuty()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        var duty = new AstronautDuty 
        { 
            PersonId = person.Id, 
            Rank = "Captain", 
            DutyTitle = "Commander", 
            DutyStartDate = DateTime.Now 
        };

        // Act
        var result = await _repository.CreateAsync(duty);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        
        var dbDuty = await _context.AstronautDuties.FindAsync(result.Id);
        dbDuty.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDuty()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        var duty = new AstronautDuty 
        { 
            PersonId = person.Id, 
            Rank = "Captain", 
            DutyTitle = "Commander", 
            DutyStartDate = DateTime.Now 
        };
        _context.AstronautDuties.Add(duty);
        await _context.SaveChangesAsync();

        duty.DutyEndDate = DateTime.Now;

        // Act
        var result = await _repository.UpdateAsync(duty);

        // Assert
        result.DutyEndDate.Should().NotBeNull();
        
        var dbDuty = await _context.AstronautDuties.FindAsync(duty.Id);
        dbDuty!.DutyEndDate.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

