using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Domain.Data;
using StargateAPI.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace StargateAPI.Tests.Integration;

public class AstronautDutyControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AstronautDutyControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = Guid.NewGuid().ToString();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StargateContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<StargateContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAstronautDuties_WhenPersonDoesNotExist_ReturnsNotFound()
    {
        // Act
        var encodedName = Uri.EscapeDataString("NonExistent");
        var response = await _client.GetAsync($"/api/astronautduty/{encodedName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAstronautDuty_WhenPersonExists_CreatesDuty()
    {
        // Arrange
        var createPersonRequest = new CreatePersonDto { Name = "John Doe" };
        await _client.PostAsJsonAsync("/api/person", createPersonRequest);

        var createDutyRequest = new CreateAstronautDutyDto
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AstronautDutyDto>>();
        content!.Data.Should().NotBeNull();
        content.Data!.DutyTitle.Should().Be("Commander");
    }

    [Fact]
    public async Task CreateAstronautDuty_WhenPersonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var createDutyRequest = new CreateAstronautDutyDto
        {
            Name = "NonExistent",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAstronautDuties_WhenPersonExists_ReturnsDuties()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "John Doe" });
        var createDutyRequest = new CreateAstronautDutyDto
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now
        };
        await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest);

        // Act
        var encodedName = Uri.EscapeDataString("John Doe");
        var response = await _client.GetAsync($"/api/astronautduty/{encodedName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AstronautDutiesResponseDto>>();
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Person.Should().NotBeNull();
        content.Data.Person!.Name.Should().Be("John Doe");
        content.Data.AstronautDuties.Should().HaveCount(1);
        content.Data.AstronautDuties.Should().NotBeEmpty();
        content.Data.AstronautDuties.First().DutyTitle.Should().Be("Commander");
    }

    [Fact]
    public async Task CreateAstronautDuty_WhenNameIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var createDutyRequest = new CreateAstronautDutyDto
        {
            Name = "",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AstronautDutyDto>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("Name is required");
    }

    [Fact]
    public async Task CreateAstronautDuty_WhenRankIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "John Doe" });
        var createDutyRequest = new CreateAstronautDutyDto
        {
            Name = "John Doe",
            Rank = "",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AstronautDutyDto>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("Rank is required");
    }

    [Fact]
    public async Task CreateAstronautDuty_WhenDutyTitleIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "John Doe" });
        var createDutyRequest = new CreateAstronautDutyDto
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "",
            DutyStartDate = DateTime.Now
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AstronautDutyDto>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("DutyTitle is required");
    }

    [Fact]
    public async Task CreateAstronautDuty_WhenDuplicateDuty_ReturnsBadRequest()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "John Doe" });
        var createDutyRequest1 = new CreateAstronautDutyDto
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.Date
        };
        await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest1);

        // Act - Try to create duplicate
        var createDutyRequest2 = new CreateAstronautDutyDto
        {
            Name = "John Doe",
            Rank = "Major",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.Date // Same date and title
        };
        var response = await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AstronautDutyDto>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetAstronautDuties_WhenPersonExistsButHasNoDuties_ReturnsEmptyDutiesList()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "John Doe" });

        // Act
        var encodedName = Uri.EscapeDataString("John Doe");
        var response = await _client.GetAsync($"/api/astronautduty/{encodedName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AstronautDutiesResponseDto>>();
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Person.Should().NotBeNull();
        content.Data.Person!.Name.Should().Be("John Doe");
        content.Data.AstronautDuties.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAstronautDuty_WhenPersonHasMultipleDuties_HandlesCorrectly()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "John Doe" });
        
        // Create first duty
        var createDutyRequest1 = new CreateAstronautDutyDto
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Commander",
            DutyStartDate = DateTime.Now.AddDays(-30)
        };
        await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest1);

        // Create second duty (should end the first one)
        var createDutyRequest2 = new CreateAstronautDutyDto
        {
            Name = "John Doe",
            Rank = "Major",
            DutyTitle = "Pilot",
            DutyStartDate = DateTime.Now
        };
        var response = await _client.PostAsJsonAsync("/api/astronautduty", createDutyRequest2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verify both duties exist
        var encodedName = Uri.EscapeDataString("John Doe");
        var getResponse = await _client.GetAsync($"/api/astronautduty/{encodedName}");
        var content = await getResponse.Content.ReadFromJsonAsync<ApiResponse<AstronautDutiesResponseDto>>();
        content!.Data!.AstronautDuties.Should().HaveCount(2);
        content.Data.AstronautDuties.First(d => d.DutyTitle == "Pilot").DutyEndDate.Should().BeNull();
    }

    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StargateContext>();
        context.Database.EnsureDeleted();
        _client.Dispose();
    }
}

