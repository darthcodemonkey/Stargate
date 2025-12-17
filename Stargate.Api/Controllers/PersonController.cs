using Microsoft.AspNetCore.Mvc;
using StargateAPI.Domain.Interfaces;
using StargateAPI.DTOs;
using System.Net;

namespace StargateAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PersonController> _logger;

    public PersonController(IPersonService personService, ILogger<PersonController> logger)
    {
        _personService = personService;
        _logger = logger;
    }

    /// <summary>
    /// Get all people
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PersonDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeople(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /api/person - Retrieving all people");
            
            var people = await _personService.GetAllPeopleAsync(cancellationToken);
            var peopleDto = people.ToDto();
            
            var response = new ApiResponse<List<PersonDto>>
            {
                Success = true,
                Message = "People retrieved successfully",
                ResponseCode = (int)HttpStatusCode.OK,
                Data = peopleDto
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving people");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<List<PersonDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving people",
                    ResponseCode = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// Get person by name
    /// </summary>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonByName(string name, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /api/person/{Name} - Retrieving person by name", name);
            
            var person = await _personService.GetPersonByNameAsync(name, cancellationToken);
            
            if (person == null)
            {
                return NotFound(new ApiResponse<PersonDto>
                {
                    Success = false,
                    Message = $"Person with name '{name}' not found",
                    ResponseCode = StatusCodes.Status404NotFound
                });
            }
            
            var response = new ApiResponse<PersonDto>
            {
                Success = true,
                Message = "Person retrieved successfully",
                ResponseCode = (int)HttpStatusCode.OK,
                Data = person.ToDto()
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving person: {Name}", name);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<PersonDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the person",
                    ResponseCode = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// Create a new person
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePerson([FromBody] CreatePersonDto request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("POST /api/person - Creating person: {Name}", request.Name);
            
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ApiResponse<PersonDto>
                {
                    Success = false,
                    Message = "Name is required",
                    ResponseCode = StatusCodes.Status400BadRequest
                });
            }
            
            var person = await _personService.CreatePersonAsync(request.Name, cancellationToken);
            
            var response = new ApiResponse<PersonDto>
            {
                Success = true,
                Message = "Person created successfully",
                ResponseCode = StatusCodes.Status201Created,
                Data = person.ToDto()
            };
            
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation creating person: {Name}", request.Name);
            return BadRequest(new ApiResponse<PersonDto>
            {
                Success = false,
                Message = ex.Message,
                ResponseCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating person: {Name}", request.Name);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<PersonDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the person",
                    ResponseCode = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// Update a person by name
    /// </summary>
    [HttpPut("{name}")]
    [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePerson(string name, [FromBody] UpdatePersonDto request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("PUT /api/person/{Name} - Updating person", name);
            
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ApiResponse<PersonDto>
                {
                    Success = false,
                    Message = "Name is required",
                    ResponseCode = StatusCodes.Status400BadRequest
                });
            }
            
            var person = await _personService.UpdatePersonAsync(name, request.Name, cancellationToken);
            
            var response = new ApiResponse<PersonDto>
            {
                Success = true,
                Message = "Person updated successfully",
                ResponseCode = (int)HttpStatusCode.OK,
                Data = person.ToDto()
            };
            
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Person not found for update: {Name}", name);
            return NotFound(new ApiResponse<PersonDto>
            {
                Success = false,
                Message = ex.Message,
                ResponseCode = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation updating person: {Name}", name);
            return BadRequest(new ApiResponse<PersonDto>
            {
                Success = false,
                Message = ex.Message,
                ResponseCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating person: {Name}", name);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<PersonDto>
                {
                    Success = false,
                    Message = "An error occurred while updating the person",
                    ResponseCode = StatusCodes.Status500InternalServerError
                });
        }
    }
}
