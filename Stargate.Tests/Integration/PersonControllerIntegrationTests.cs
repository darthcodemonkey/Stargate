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

public class PersonControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PersonControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = Guid.NewGuid().ToString();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StargateContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add in-memory database with unique name per test class
                services.AddDbContext<StargateContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetPeople_ReturnsEmptyList_WhenNoPeopleExist()
    {
        // Act
        var response = await _client.GetAsync("/api/person");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<List<PersonDto>>>();
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePerson_ThenGetPerson_ReturnsCreatedPerson()
    {
        // Arrange
        var createRequest = new CreatePersonDto { Name = "John Doe" };

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/person", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        created!.Data.Should().NotBeNull();
        var personId = created.Data!.PersonId;

        // Act - Get (URL encode the name)
        var encodedName = Uri.EscapeDataString("John Doe");
        var getResponse = await _client.GetAsync($"/api/person/{encodedName}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var person = await getResponse.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();

        // Assert
        person!.Data.Should().NotBeNull();
        person.Data!.Name.Should().Be("John Doe");
        person.Data.PersonId.Should().Be(personId);
    }

    [Fact]
    public async Task CreatePerson_WhenDuplicate_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreatePersonDto { Name = "John Doe" };
        await _client.PostAsJsonAsync("/api/person", createRequest);

        // Act
        var response = await _client.PostAsJsonAsync("/api/person", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        content!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePerson_WhenPersonExists_UpdatesPerson()
    {
        // Arrange
        var createRequest = new CreatePersonDto { Name = "Old Name" };
        await _client.PostAsJsonAsync("/api/person", createRequest);

        var updateRequest = new UpdatePersonDto { Name = "New Name" };

        // Act
        var encodedOldName = Uri.EscapeDataString("Old Name");
        var response = await _client.PutAsJsonAsync($"/api/person/{encodedOldName}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        content!.Data.Should().NotBeNull();
        content.Data!.Name.Should().Be("New Name");

        // Verify by getting
        var encodedNewName = Uri.EscapeDataString("New Name");
        var getResponse = await _client.GetAsync($"/api/person/{encodedNewName}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPerson_WhenPersonDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/person/NonExistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePerson_WhenNameIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreatePersonDto { Name = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/person", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("Name is required");
    }

    [Fact]
    public async Task CreatePerson_WhenNameIsNull_ReturnsBadRequest()
    {
        // Arrange - Send JSON with null name explicitly
        var json = "{\"name\": null}";
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/person", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
        // Model binding may reject null before reaching controller, so just verify it's a client error
        ((int)response.StatusCode).Should().BeInRange(400, 499);
    }

    [Fact]
    public async Task CreatePerson_WhenNameIsWhitespace_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreatePersonDto { Name = "   " };

        // Act
        var response = await _client.PostAsJsonAsync("/api/person", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("Name is required");
    }

    [Fact]
    public async Task UpdatePerson_WhenNameIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreatePersonDto { Name = "Existing Person" };
        await _client.PostAsJsonAsync("/api/person", createRequest);

        var updateRequest = new UpdatePersonDto { Name = "" };
        var encodedName = Uri.EscapeDataString("Existing Person");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/person/{encodedName}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("Name is required");
    }

    [Fact]
    public async Task UpdatePerson_WhenNewNameAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "Person One" });
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "Person Two" });

        var updateRequest = new UpdatePersonDto { Name = "Person Two" };
        var encodedName = Uri.EscapeDataString("Person One");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/person/{encodedName}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdatePerson_WhenPersonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdatePersonDto { Name = "New Name" };
        var encodedName = Uri.EscapeDataString("NonExistent");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/person/{encodedName}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetPeople_ReturnsListWithMultiplePeople()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "Person One" });
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "Person Two" });
        await _client.PostAsJsonAsync("/api/person", new CreatePersonDto { Name = "Person Three" });

        // Act
        var response = await _client.GetAsync("/api/person");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<List<PersonDto>>>();
        content!.Success.Should().BeTrue();
        content.Data.Should().HaveCount(3);
        content.Data!.Select(p => p.Name).Should().Contain("Person One", "Person Two", "Person Three");
    }

    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StargateContext>();
        context.Database.EnsureDeleted();
        _client.Dispose();
    }
}

