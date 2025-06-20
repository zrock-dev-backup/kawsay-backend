using Application.Core;
using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface ISchedulingEngineService
{
    Task<Result<AvailableSlotsResponseDto>> GetAvailableSlotsAsync(CourseRequirementEntity requirement,
        AvailableSlotsRequestDto slotRequest);
}