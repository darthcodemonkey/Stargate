namespace StargateAPI.DTOs;

public class AstronautDutiesResponseDto
{
    public PersonDto? Person { get; set; }
    public List<AstronautDutyDto> AstronautDuties { get; set; } = new();
}

