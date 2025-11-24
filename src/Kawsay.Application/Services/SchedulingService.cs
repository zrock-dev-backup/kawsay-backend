using Application.Core;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Kawsay.Domain.Enums;

namespace Application.Services;

public class SchedulingService(
    IStagedPlacementRepository placementRepo,
    ICourseRequirementRepository requirementRepo,
    IClassRepository classRepo,
    ITimetableRepository timetableRepo,
    IUnitOfWork unitOfWork // REFACTORED: Using Abstract UnitOfWork
    )
{
    public async Task<Result<StagedPlacementDto>> StagePlacementAsync(CreateStagedPlacementRequest request)
    {
        var requirement = await requirementRepo.GetByIdAsync(request.RequirementId);
        if (requirement == null)
            return Result<StagedPlacementDto>.Failure(Error.NotFound("Requirement.NotFound", "Requirement not found"));

        var entity = new StagedPlacementEntity
        {
            CourseRequirementId = request.RequirementId,
            DayId = request.DayId,
            StartPeriodId = request.StartPeriodId,
            Length = requirement.DurationInPeriods
        };

        await placementRepo.AddAsync(entity);
        await placementRepo.SaveChangesAsync();

        return Result<StagedPlacementDto>.Success(new StagedPlacementDto(
            entity.Id,
            entity.CourseRequirementId,
            requirement.Course.Name,
            requirement.Course.Code,
            entity.DayId,
            entity.StartPeriodId,
            entity.Length
        ));
    }

    public async Task<Result> DeleteStagedPlacementAsync(int id)
    {
        var entity = await placementRepo.GetByIdAsync(id);
        if (entity == null) return Result.Failure(Error.NotFound("Placement.NotFound", "Placement not found"));

        await placementRepo.DeleteAsync(entity);
        await placementRepo.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<FinalizeScheduleResponse>> FinalizeScheduleAsync(int timetableId)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var timetable = await timetableRepo.GetByIdAsync(timetableId);
            if (timetable == null) 
                return Result<FinalizeScheduleResponse>.Failure(Error.NotFound("Timetable", "Not found"));

            var stagedItems = await placementRepo.GetByTimetableIdAsync(timetableId);
            if (stagedItems.Count == 0)
                return Result<FinalizeScheduleResponse>.Success(new FinalizeScheduleResponse("No items to finalize.", 0));

            var classesCreated = 0;
            
            // Map DayId (DB ID) to DayOfWeek Name for date calculation
            var dayIdToNameMap = timetable.Days.ToDictionary(d => d.Id, d => d.Name);

            foreach (var item in stagedItems)
            {
                var req = item.CourseRequirement;
                
                var newClass = new ClassEntity
                {
                    TimetableId = timetableId,
                    CourseId = req.CourseId,
                    TeacherId = req.PreferredTeacherId,
                    StudentGroupId = req.StudentGroupId,
                    SectionId = req.SectionId,
                    ClassType = req.Priority == CourseRequirementPriority.High ? Domain.Enums.ClassType.Lab : Domain.Enums.ClassType.Masterclass,
                    Length = item.Length,
                    Frequency = req.FrequencyPerWeek,
                    Capacity = req.RequiredCapacity ?? 30, 
                    StartDate = req.EffectiveDateRange.StartDate,
                    EndDate = req.EffectiveDateRange.EndDate
                };

                // FIX: Resolve "DayId" error by calculating concrete "Date"s for ClassOccurrenceEntity
                if (dayIdToNameMap.TryGetValue(item.DayId, out var dayName) && 
                    Enum.TryParse<DayOfWeek>(dayName, true, out var targetDayOfWeek))
                {
                    // Iterate from StartDate to EndDate and create an occurrence for every matching day of week
                    for (var date = newClass.StartDate.Value; date <= newClass.EndDate.Value; date = date.AddDays(1))
                    {
                        if (date.DayOfWeek == targetDayOfWeek)
                        {
                            newClass.ClassOccurrences.Add(new ClassOccurrenceEntity
                            {
                                Date = date, // Correct property
                                StartPeriodId = item.StartPeriodId
                            });
                        }
                    }
                }

                // Add PeriodPreference to store the pattern (DayId is valid here)
                newClass.PeriodPreferences.Add(new PeriodPreferenceEntity
                {
                    DayId = item.DayId, 
                    StartPeriodId = item.StartPeriodId
                });

                await classRepo.AddAsync(newClass);
                classesCreated++;
            }

            await placementRepo.DeleteAllForTimetableAsync(timetableId);
            await unitOfWork.CommitTransactionAsync();

            return Result<FinalizeScheduleResponse>.Success(new FinalizeScheduleResponse("Schedule finalized successfully.", classesCreated));
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result<FinalizeScheduleResponse>.Failure(Error.Failure("Finalization.Failed", ex.Message));
        }
    }
}
