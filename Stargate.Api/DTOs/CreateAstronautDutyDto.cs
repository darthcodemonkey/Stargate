namespace StargateAPI.DTOs;

public class CreateAstronautDutyDto
{
    public string Name { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string DutyTitle { get; set; } = string.Empty;
    public DateTime DutyStartDate { get; set; }
}

