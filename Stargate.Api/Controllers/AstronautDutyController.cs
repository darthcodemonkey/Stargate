using Microsoft.AspNetCore.Mvc;
using StargateAPI.Domain.Interfaces;
using StargateAPI.DTOs;
using System.Net;

namespace StargateAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AstronautDutyController : ControllerBase
{
    private readonly IAstronautDutyService _astronautDutyService;
    private readonly ILogger<AstronautDutyController> _logger;

    public AstronautDutyController(IAstronautDutyService astronautDutyService, ILogger<AstronautDutyController> logger)
    {
        _astronautDutyService = astronautDutyService;
        _logger = logger;
    }

    /// <summary>
    /// Get astronaut duties by person name
    /// </summary>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(ApiResponse<AstronautDutiesResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAstronautDutiesByName(string name, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /api/astronaut-duty/{Name} - Retrieving astronaut duties", name);
            
            var (person, duties) = await _astronautDutyService.GetAstronautDutiesByNameAsync(name, cancellationToken);
            
            if (person == null)
            {
                return NotFound(new ApiResponse<AstronautDutiesResponseDto>
                {
                    Success = false,
                    Message = $"Person with name '{name}' not found",
                    ResponseCode = StatusCodes.Status404NotFound
                });
            }
            
            var responseDto = new AstronautDutiesResponseDto
            {
                Person = person.ToDto(),
                AstronautDuties = duties.ToDto()
            };
            
            var response = new ApiResponse<AstronautDutiesResponseDto>
            {
                Success = true,
                Message = "Astronaut duties retrieved successfully",
                ResponseCode = (int)HttpStatusCode.OK,
                Data = responseDto
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving astronaut duties for: {Name}", name);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<AstronautDutiesResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving astronaut duties",
                    ResponseCode = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// Create a new astronaut duty
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AstronautDutyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDutyDto request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "POST /api/astronaut-duty - Creating duty for: {Name}, Title: {Title}",
                request.Name, request.DutyTitle);
            
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ApiResponse<AstronautDutyDto>
                {
                    Success = false,
                    Message = "Name is required",
                    ResponseCode = StatusCodes.Status400BadRequest
                });
            }
            
            if (string.IsNullOrWhiteSpace(request.Rank))
            {
                return BadRequest(new ApiResponse<AstronautDutyDto>
                {
                    Success = false,
                    Message = "Rank is required",
                    ResponseCode = StatusCodes.Status400BadRequest
                });
            }
            
            if (string.IsNullOrWhiteSpace(request.DutyTitle))
            {
                return BadRequest(new ApiResponse<AstronautDutyDto>
                {
                    Success = false,
                    Message = "DutyTitle is required",
                    ResponseCode = StatusCodes.Status400BadRequest
                });
            }
            
            var duty = await _astronautDutyService.CreateAstronautDutyAsync(
                request.Name,
                request.Rank,
                request.DutyTitle,
                request.DutyStartDate,
                cancellationToken);
            
            var response = new ApiResponse<AstronautDutyDto>
            {
                Success = true,
                Message = "Astronaut duty created successfully",
                ResponseCode = StatusCodes.Status201Created,
                Data = duty.ToDto()
            };
            
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Person not found for creating duty: {Name}", request.Name);
            return NotFound(new ApiResponse<AstronautDutyDto>
            {
                Success = false,
                Message = ex.Message,
                ResponseCode = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation creating duty: {Name}", request.Name);
            return BadRequest(new ApiResponse<AstronautDutyDto>
            {
                Success = false,
                Message = ex.Message,
                ResponseCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating astronaut duty for: {Name}", request.Name);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<AstronautDutyDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the astronaut duty",
                    ResponseCode = StatusCodes.Status500InternalServerError
                });
        }
    }
}
