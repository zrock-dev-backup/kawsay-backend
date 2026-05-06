namespace Domain.Entities.availability;

public class AvailabilityEntity
{
    public int Id { get; set; }
    public string EntityType { get; set; } = "faculty";
    public int EntityId { get; set; }
    
    public AvailabilityStatus AvailabilityStatus { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // ✅ Adapted to support ranges directly
    public int Day { get; set; }
    public int PeriodStart { get; set; } 
    public int PeriodEnd { get; set; }   
}
