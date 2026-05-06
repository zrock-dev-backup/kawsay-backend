using Application.Core;
using Application.Interfaces.Persistence;
using Application.Models.Faculty;
using Domain.Entities.availability;

namespace Application.Services;

public class FacultyAvailabilityService(IFacultyAvailabilityRepository repo, IUnitOfWork unitOfWork)
{
    public async Task<Result<List<AvailabilityResponseDto>>> AddAvailabilitiesAsync(int facultyId, List<AvailabilityRequestDto> requests)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var entities = requests.Select(dto => new AvailabilityEntity
            {
                EntityType = "faculty",
                EntityId = facultyId,
                Status = dto.Status,
                AvailabilityStatus = MapStatus(dto.Status),
                Day = dto.Range.DayId,
                PeriodStart = dto.Range.PeriodStartId,
                PeriodEnd = dto.Range.PeriodEndId
            }).ToList();

            await repo.AddRangeAsync(entities);
            await unitOfWork.CommitTransactionAsync();

            return Result<List<AvailabilityResponseDto>>.Success(entities.Select(MapToResponse).ToList());
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result<List<AvailabilityResponseDto>>.Failure(Error.Failure("Db.Error", ex.Message));
        }
    }

    public async Task<Result<List<AvailabilityResponseDto>>> GetByFacultyIdAsync(int facultyId)
    {
        var entities = await repo.GetByFacultyIdAsync(facultyId);
        return Result<List<AvailabilityResponseDto>>.Success(entities.Select(MapToResponse).ToList());
    }

    public async Task<Result<List<FacultyAvailabilityGroupResponseDto>>> GetAllGroupedAsync()
    {
        var entities = await repo.GetAllAsync();
        var grouped = entities.GroupBy(e => e.EntityId)
            .Select(g => new FacultyAvailabilityGroupResponseDto(
                g.Key, 
                g.Select(MapToResponse).ToList()))
            .ToList();
            
        return Result<List<FacultyAvailabilityGroupResponseDto>>.Success(grouped);
    }

    public async Task<Result<AvailabilityResponseDto>> UpdateAsync(int facultyId, int id, AvailabilityRequestDto dto)
    {
        var entity = await repo.GetByIdAsync(id);
        if (entity == null || entity.EntityId != facultyId) return Result<AvailabilityResponseDto>.Failure(Error.NotFound("Availability.NotFound", "Record not found"));

        entity.Status = dto.Status;
        entity.AvailabilityStatus = MapStatus(dto.Status);
        entity.Day = dto.Range.DayId;
        entity.PeriodStart = dto.Range.PeriodStartId;
        entity.PeriodEnd = dto.Range.PeriodEndId;

        await unitOfWork.SaveChangesAsync();
        return Result<AvailabilityResponseDto>.Success(MapToResponse(entity));
    }

    public async Task<Result<AvailabilityResponseDto>> PatchAsync(int facultyId, int id, AvailabilityPatchRequestDto dto)
    {
        var entity = await repo.GetByIdAsync(id);
        if (entity == null || entity.EntityId != facultyId) return Result<AvailabilityResponseDto>.Failure(Error.NotFound("Availability.NotFound", "Record not found"));

        if (!string.IsNullOrEmpty(dto.Status))
        {
            entity.Status = dto.Status;
            entity.AvailabilityStatus = MapStatus(dto.Status);
        }
        
        if (dto.Range != null)
        {
            entity.Day = dto.Range.DayId;
            entity.PeriodStart = dto.Range.PeriodStartId;
            entity.PeriodEnd = dto.Range.PeriodEndId;
        }

        await unitOfWork.SaveChangesAsync();
        return Result<AvailabilityResponseDto>.Success(MapToResponse(entity));
    }

    public async Task<Result> DeleteAsync(int facultyId, int id)
    {
        var entity = await repo.GetByIdAsync(id);
        if (entity == null || entity.EntityId != facultyId) return Result.Failure(Error.NotFound("Availability.NotFound", "Record not found"));

        await repo.DeleteAsync(entity);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    // --- Helpers ---
    private static AvailabilityStatus MapStatus(string status) => 
        status.Equals("free", StringComparison.OrdinalIgnoreCase) ? AvailabilityStatus.Available : AvailabilityStatus.Scheduled;

    private static string FormatStatus(AvailabilityStatus status) => 
        status == AvailabilityStatus.Available ? "free" : "scheduled";

    private static AvailabilityResponseDto MapToResponse(AvailabilityEntity entity) => new(
        entity.Id,
        FormatStatus(entity.AvailabilityStatus), // Or entity.Status directly
        new AvailabilityRangeDto(entity.Day, entity.PeriodStart, entity.PeriodEnd)
    );
}
