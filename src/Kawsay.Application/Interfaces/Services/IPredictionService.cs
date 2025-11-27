// File: src/Kawsay.Application/Interfaces/Services/IPredictionService.cs
using Application.Core;
using Application.DTOs;

namespace Application.Interfaces.Services;

public interface IPredictionService
{
    Task<Result<List<StudentPredictionDto>>> PredictBatchAsync(List<StudentGradeInputDto> inputs);
}
