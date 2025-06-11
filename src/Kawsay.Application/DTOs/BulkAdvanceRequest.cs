namespace Application.DTOs;

public record BulkAdvanceRequest(int TimetableId, List<int> StudentIds);
