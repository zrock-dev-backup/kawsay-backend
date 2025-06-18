namespace Application.Features.Scheduling.Models
{
    public class ValidSlotDto
    {
        public int DayId { get; set; }
        public int StartPeriodId { get; set; }
        public SlotType Type { get; set; }
        public double GuidanceScore { get; set; }
    }
}
