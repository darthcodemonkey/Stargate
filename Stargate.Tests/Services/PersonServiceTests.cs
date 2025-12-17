using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Domain.Interfaces;
using StargateAPI.Domain.Models;
using StargateAPI.Services.Services;

namespace StargateAPI.Tests.Services;

public class PersonServiceTests
{
    private readonly Mock<IPersonRepository> _mockRepository;
    private readonly Mock<ILogger<PersonService>> _mockLogger;
    private readonly PersonService _service;

    public PersonServiceTests()
    {
        _mockRepository = new Mock<IPersonRepository>();
        _mockLogger = new Mock<ILogger<PersonService>>();
        _service = new PersonService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetPersonByNameAsync_WhenPersonExists_ReturnsPerson()
    {
        // Arrange
        var name = "John Doe";
        var expectedPerson = new Person { Id = 1, Name = name };
        _mockRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerson);

        // Act
        var result = await _service.GetPersonByNameAsync(name);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(name);
        result.Id.Should().Be(1);
        _mockRepository.Verify(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPersonByNameAsync_WhenPersonDoesNotExist_ReturnsNull()
    {
        // Arrange
        var name = "NonExistent";
        _mockRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _service.GetPersonByNameAsync(name);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllPeopleAsync_ReturnsAllPeople()
    {
        // Arrange
        var expectedPeople = new List<Person>
        {
            new Person { Id = 1, Name = "John Doe" },
            new Person { Id = 2, Name = "Jane Doe" }
        };
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPeople);

        // Act
        var result = await _service.GetAllPeopleAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "John Doe");
        result.Should().Contain(p => p.Name == "Jane Doe");
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePersonAsync_WhenPersonDoesNotExist_CreatesPerson()
    {
        // Arrange
        var name = "New Person";
        _mockRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);
        
        var createdPerson = new Person { Id = 1, Name = name };
        _mockRepository.Setup(r => r.CreateAsync(It.Is<Person>(p => p.Name == name), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPerson);

        // Act
        var result = await _service.CreatePersonAsync(name);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Id.Should().Be(1);
        _mockRepository.Verify(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePersonAsync_WhenPersonAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var name = "Existing Person";
        var existingPerson = new Person { Id = 1, Name = name };
        _mockRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);

        // Act
        Func<Task> act = async () => await _service.CreatePersonAsync(name);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"A person with the name '{name}' already exists.");
        _mockRepository.Verify(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePersonAsync_WhenPersonExists_UpdatesPerson()
    {
        // Arrange
        var oldName = "Old Name";
        var newName = "New Name";
        var existingPerson = new Person { Id = 1, Name = oldName };
        
        _mockRepository.Setup(r => r.GetByNameAsync(oldName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);
        _mockRepository.Setup(r => r.GetByNameAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);
        
        var updatedPerson = new Person { Id = 1, Name = newName };
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPerson);

        // Act
        var result = await _service.UpdatePersonAsync(oldName, newName);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(newName);
        result.Id.Should().Be(1);
        _mockRepository.Verify(r => r.GetByNameAsync(oldName, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GetByNameAsync(newName, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Person>(p => p.Name == newName), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePersonAsync_WhenPersonDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var name = "NonExistent";
        var newName = "New Name";
        _mockRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        Func<Task> act = async () => await _service.UpdatePersonAsync(name, newName);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Person with name '{name}' not found.");
        _mockRepository.Verify(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePersonAsync_WhenNewNameAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var oldName = "Old Name";
        var newName = "Existing Name";
        var existingPerson = new Person { Id = 1, Name = oldName };
        var personWithNewName = new Person { Id = 2, Name = newName };
        
        _mockRepository.Setup(r => r.GetByNameAsync(oldName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);
        _mockRepository.Setup(r => r.GetByNameAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(personWithNewName);

        // Act
        Func<Task> act = async () => await _service.UpdatePersonAsync(oldName, newName);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"A person with the name '{newName}' already exists.");
        _mockRepository.Verify(r => r.GetByNameAsync(oldName, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GetByNameAsync(newName, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePersonAsync_WhenNameIsUnchanged_UpdatesSuccessfully()
    {
        // Arrange
        var name = "Same Name";
        var existingPerson = new Person { Id = 1, Name = name };
        
        _mockRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);
        
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);

        // Act
        var result = await _service.UpdatePersonAsync(name, name);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        _mockRepository.Verify(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()), Times.Once);
        // Should not check for duplicate when name is unchanged
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

