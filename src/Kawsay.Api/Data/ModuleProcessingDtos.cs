namespace Api.Controllers;

public record BulkAdvanceRequest(int TimetableId, List<int> StudentIds);
public record BulkActionResponse(string Message, int ProcessedCount);
