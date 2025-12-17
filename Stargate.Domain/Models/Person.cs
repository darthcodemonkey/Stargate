namespace StargateAPI.Domain.Models;

public class Person
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public virtual AstronautDetail? AstronautDetail { get; set; }

    public virtual ICollection<AstronautDuty> AstronautDuties { get; set; } = new HashSet<AstronautDuty>();
}

