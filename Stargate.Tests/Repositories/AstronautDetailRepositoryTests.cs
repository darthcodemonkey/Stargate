using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Domain.Data;
using StargateAPI.Domain.Models;
using StargateAPI.Domain.Repositories;

namespace StargateAPI.Tests.Repositories;

public class AstronautDetailRepositoryTests : IDisposable
{
    private readonly StargateContext _context;
    private readonly AstronautDetailRepository _repository;

    public AstronautDetailRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StargateContext(options);
        _repository = new AstronautDetailRepository(_context);
    }

    [Fact]
    public async Task GetByPersonIdAsync_WhenDetailExists_ReturnsDetail()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

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
        var result = await _repository.GetByPersonIdAsync(person.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentRank.Should().Be("Captain");
        result.CurrentDutyTitle.Should().Be("Commander");
    }

    [Fact]
    public async Task GetByPersonIdAsync_WhenDetailDoesNotExist_ReturnsNull()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPersonIdAsync(person.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_CreatesDetail()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        var detail = new AstronautDetail 
        { 
            PersonId = person.Id, 
            CurrentRank = "Captain", 
            CurrentDutyTitle = "Commander",
            CareerStartDate = DateTime.Now
        };

        // Act
        var result = await _repository.CreateAsync(detail);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        
        var dbDetail = await _context.AstronautDetails.FindAsync(result.Id);
        dbDetail.Should().NotBeNull();
        dbDetail!.CurrentRank.Should().Be("Captain");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDetail()
    {
        // Arrange
        var person = new Person { Name = "John Doe" };
        _context.People.Add(person);
        await _context.SaveChangesAsync();

        var detail = new AstronautDetail 
        { 
            PersonId = person.Id, 
            CurrentRank = "Captain", 
            CurrentDutyTitle = "Commander",
            CareerStartDate = DateTime.Now
        };
        _context.AstronautDetails.Add(detail);
        await _context.SaveChangesAsync();

        detail.CurrentRank = "Major";

        // Act
        var result = await _repository.UpdateAsync(detail);

        // Assert
        result.CurrentRank.Should().Be("Major");
        
        var dbDetail = await _context.AstronautDetails.FindAsync(detail.Id);
        dbDetail!.CurrentRank.Should().Be("Major");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

