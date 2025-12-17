using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Domain.Data;
using StargateAPI.Domain.Models;
using StargateAPI.Domain.Repositories;

namespace StargateAPI.Tests.Repositories;

public class PersonRepositoryTests : IDisposable
{
    private readonly StargateContext _context;
    private readonly PersonRepository _repository;

    public PersonRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StargateContext(options);
        _repository = new PersonRepository(_context);
    }

    [Fact]
    public async Task GetByNameAsync_WhenPersonExists_ReturnsPerson()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("John Doe");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByNameAsync_WhenPersonDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPeople()
    {
        // Arrange
        _context.People.AddRange(
            new Person { Name = "John Doe" },
            new Person { Name = "Jane Doe" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_CreatesPerson()
    {
        // Arrange
        var person = new Person { Name = "New Person" };

        // Act
        var result = await _repository.CreateAsync(person);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Person");
        
        var dbPerson = await _context.People.FindAsync(result.Id);
        dbPerson.Should().NotBeNull();
        dbPerson!.Name.Should().Be("New Person");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPerson()
    {
        // Arrange
        var person = new Person { Name = "Old Name" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        person.Name = "New Name";

        // Act
        var result = await _repository.UpdateAsync(person);

        // Assert
        result.Name.Should().Be("New Name");
        
        var dbPerson = await _context.People.FindAsync(person.Id);
        dbPerson!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task GetByNameAsync_IncludesAstronautDetail()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync(); // Save first to get the ID
        
        var detail = new AstronautDetail 
        { 
            PersonId = person.Id, 
            CurrentRank = "Captain", 
            CurrentDutyTitle = "Commander",
            CareerStartDate = DateTime.Now
        };
        _context.AstronautDetails.Add(detail);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("John Doe");

        // Assert
        result.Should().NotBeNull();
        result!.AstronautDetail.Should().NotBeNull();
        result.AstronautDetail!.CurrentRank.Should().Be("Captain");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

