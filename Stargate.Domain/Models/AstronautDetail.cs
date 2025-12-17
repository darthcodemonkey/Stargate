namespace StargateAPI.Domain.Models;

public class AstronautDetail
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public string CurrentRank { get; set; } = string.Empty;

    public string CurrentDutyTitle { get; set; } = string.Empty;

    public DateTime CareerStartDate { get; set; }

    public DateTime? CareerEndDate { get; set; }

    public virtual Person Person { get; set; } = null!;
}

