using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Domain.Interfaces;
using StargateAPI.Domain.Models;
using StargateAPI.Services.Services;

namespace StargateAPI.Tests.Services;

public class AstronautDutyServiceTests
{
    private readonly Mock<IPersonRepository> _mockPersonRepository;
    private readonly Mock<IAstronautDutyRepository> _mockDutyRepository;
    private readonly Mock<IAstronautDetailRepository> _mockDetailRepository;
    private readonly Mock<ILogger<AstronautDutyService>> _mockLogger;
    private readonly AstronautDutyService _service;

    public AstronautDutyServiceTests()
    {
        _mockPersonRepository = new Mock<IPersonRepository>();
        _mockDutyRepository = new Mock<IAstronautDutyRepository>();
        _mockDetailRepository = new Mock<IAstronautDetailRepository>();
        _mockLogger = new Mock<ILogger<AstronautDutyService>>();
        _service = new AstronautDutyService(
            _mockPersonRepository.Object,
            _mockDutyRepository.Object,
            _mockDetailRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetAstronautDutiesByNameAsync_WhenPersonExists_ReturnsPersonAndDuties()
    {
        // Arrange
        var name = "John Doe";
        var person = new Person { Id = 1, Name = name };
        var duties = new List<AstronautDuty>
        {
            new AstronautDuty { Id = 1, PersonId = 1, Rank = "Captain", DutyTitle = "Commander", DutyStartDate = DateTime.Now.AddDays(-30) }
        };

        _mockPersonRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);
        _mockDutyRepository.Setup(r => r.GetByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(duties);

        // Act
        var (resultPerson, resultDuties) = await _service.GetAstronautDutiesByNameAsync(name);

        // Assert
        resultPerson.Should().NotBeNull();
        resultPerson!.Name.Should().Be(name);
        resultDuties.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAstronautDutiesByNameAsync_WhenPersonDoesNotExist_ReturnsNullAndEmptyDuties()
    {
        // Arrange
        var name = "NonExistent";
        _mockPersonRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        var (resultPerson, resultDuties) = await _service.GetAstronautDutiesByNameAsync(name);

        // Assert
        resultPerson.Should().BeNull();
        resultDuties.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAstronautDutyAsync_WhenPersonExistsAndNoCurrentDuty_CreatesNewDutyAndDetail()
    {
        // Arrange
        var name = "John Doe";
        var rank = "Captain";
        var dutyTitle = "Commander";
        var startDate = DateTime.Now;
        var person = new Person { Id = 1, Name = name };

        _mockPersonRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);
        _mockDutyRepository.Setup(r => r.GetByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AstronautDuty>());
        _mockDutyRepository.Setup(r => r.GetCurrentDutyByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDuty?)null);
        _mockDetailRepository.Setup(r => r.GetByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDetail?)null);

        var newDuty = new AstronautDuty { Id = 1, PersonId = 1, Rank = rank, DutyTitle = dutyTitle, DutyStartDate = startDate };
        _mockDutyRepository.Setup(r => r.CreateAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newDuty);

        var newDetail = new AstronautDetail { Id = 1, PersonId = 1, CurrentRank = rank, CurrentDutyTitle = dutyTitle, CareerStartDate = startDate };
        _mockDetailRepository.Setup(r => r.CreateAsync(It.IsAny<AstronautDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newDetail);

        // Act
        var result = await _service.CreateAstronautDutyAsync(name, rank, dutyTitle, startDate);

        // Assert
        result.Should().NotBeNull();
        result.DutyTitle.Should().Be(dutyTitle);
        result.DutyEndDate.Should().BeNull(); // Business Rule: Current Duty has no DutyEndDate
        _mockDutyRepository.Verify(r => r.CreateAsync(It.Is<AstronautDuty>(d => d.DutyEndDate == null), It.IsAny<CancellationToken>()), Times.Once);
        _mockDetailRepository.Verify(r => r.CreateAsync(It.IsAny<AstronautDetail>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAstronautDutyAsync_WhenPersonDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var name = "NonExistent";
        _mockPersonRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        Func<Task> act = async () => await _service.CreateAstronautDutyAsync(name, "Rank", "Title", DateTime.Now);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Person with name '{name}' not found.");
    }

    [Fact]
    public async Task CreateAstronautDutyAsync_WhenDuplicateDuty_ThrowsInvalidOperationException()
    {
        // Arrange
        var name = "John Doe";
        var rank = "Captain";
        var dutyTitle = "Commander";
        var startDate = DateTime.Now.Date;
        var person = new Person { Id = 1, Name = name };
        var existingDuty = new AstronautDuty 
        { 
            Id = 1, 
            PersonId = 1, 
            DutyTitle = dutyTitle, 
            DutyStartDate = startDate 
        };

        _mockPersonRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);
        _mockDutyRepository.Setup(r => r.GetByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AstronautDuty> { existingDuty });

        // Act
        Func<Task> act = async () => await _service.CreateAstronautDutyAsync(name, rank, dutyTitle, startDate);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"An astronaut duty with title '{dutyTitle}' and start date '{startDate:yyyy-MM-dd}' already exists for this person.");
    }

    [Fact]
    public async Task CreateAstronautDutyAsync_WhenCurrentDutyExists_SetsEndDateToDayBeforeStartDate()
    {
        // Arrange
        var name = "John Doe";
        var rank = "Captain";
        var dutyTitle = "Commander";
        var newStartDate = DateTime.Now.Date;
        var person = new Person { Id = 1, Name = name };
        var currentDuty = new AstronautDuty 
        { 
            Id = 1, 
            PersonId = 1, 
            DutyTitle = "Old Title", 
            DutyStartDate = newStartDate.AddDays(-30),
            DutyEndDate = null
        };

        _mockPersonRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);
        _mockDutyRepository.Setup(r => r.GetByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AstronautDuty>());
        _mockDutyRepository.Setup(r => r.GetCurrentDutyByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentDuty);
        _mockDetailRepository.Setup(r => r.GetByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDetail?)null);

        var expectedEndDate = newStartDate.AddDays(-1).Date;
        _mockDutyRepository.Setup(r => r.UpdateAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDuty d, CancellationToken _) => d);
        _mockDutyRepository.Setup(r => r.CreateAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AstronautDuty { Id = 2, PersonId = 1, Rank = rank, DutyTitle = dutyTitle, DutyStartDate = newStartDate });
        _mockDetailRepository.Setup(r => r.CreateAsync(It.IsAny<AstronautDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AstronautDetail { Id = 1, PersonId = 1 });

        // Act
        await _service.CreateAstronautDutyAsync(name, rank, dutyTitle, newStartDate);

        // Assert
        // Business Rule: Previous Duty EndDate = day before new Duty StartDate
        _mockDutyRepository.Verify(r => r.UpdateAsync(
            It.Is<AstronautDuty>(d => d.DutyEndDate == expectedEndDate), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAstronautDutyAsync_WhenDutyTitleIsRETIRED_SetsCareerEndDate()
    {
        // Arrange
        var name = "John Doe";
        var rank = "Captain";
        var dutyTitle = "RETIRED";
        var startDate = DateTime.Now.Date;
        var person = new Person { Id = 1, Name = name };
        var existingDetail = new AstronautDetail 
        { 
            Id = 1, 
            PersonId = 1, 
            CurrentRank = "Major", 
            CurrentDutyTitle = "Commander",
            CareerStartDate = startDate.AddDays(-100)
        };

        _mockPersonRepository.Setup(r => r.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);
        _mockDutyRepository.Setup(r => r.GetByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AstronautDuty>());
        _mockDutyRepository.Setup(r => r.GetCurrentDutyByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDuty?)null);
        _mockDetailRepository.Setup(r => r.GetByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDetail);

        var expectedCareerEndDate = startDate.AddDays(-1).Date; // Business Rule: CareerEndDate = day before Retired Duty StartDate
        _mockDetailRepository.Setup(r => r.UpdateAsync(It.IsAny<AstronautDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDetail d, CancellationToken _) => d);
        _mockDutyRepository.Setup(r => r.CreateAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AstronautDuty { Id = 1, PersonId = 1 });

        // Act
        await _service.CreateAstronautDutyAsync(name, rank, dutyTitle, startDate);

        // Assert
        // Business Rule: If DutyTitle is 'RETIRED', CareerEndDate = day before Retired Duty StartDate
        _mockDetailRepository.Verify(r => r.UpdateAsync(
            It.Is<AstronautDetail>(d => d.CareerEndDate == expectedCareerEndDate), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

