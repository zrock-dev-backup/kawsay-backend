namespace Application.Features.Scheduling.Models
{
    public enum SlotType
    {
        Ideal,  // Conflict-free and matches soft preferences
        Viable  // Conflict-free but does not match soft preferences
    }
}
