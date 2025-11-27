using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Core;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.External;

public class PredictionApiClient(HttpClient httpClient, ILogger<PredictionApiClient> logger) : IPredictionService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<Result<List<StudentPredictionDto>>> PredictBatchAsync(List<StudentGradeInputDto> inputs)
    {
        try
        {
            // Map to external contract format expected by Python service
            // The Python API expects "grade_lab" and "grade_masterclass" (snake_case), 
            // while our domain uses Pascal/CamelCase.
            var payload = new
            {
                records = inputs.Select(i => new ExternalGradeRecord
                {
                    StudentId = i.StudentId,
                    CourseId = i.CourseId,
                    Semester = i.Semester,
                    GradeLab = i.GradeLab,
                    GradeMasterclass = i.GradeMasterclass
                }).ToList()
            };

            var response = await httpClient.PostAsJsonAsync("/bulk-predict", payload, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                logger.LogError("Prediction API error: {StatusCode} - {Content}", response.StatusCode, content);
                
                // Return the actual content message to help with debugging
                return Result<List<StudentPredictionDto>>.Failure(
                    Error.Failure("PredictionApi.Error", $"External service error: {response.StatusCode}. Details: {content}"));
            }

            var result = await response.Content.ReadFromJsonAsync<PredictionResponse>(_jsonOptions);
            return Result<List<StudentPredictionDto>>.Success(result?.Predictions ?? []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to Prediction API");
            return Result<List<StudentPredictionDto>>.Failure(
                Error.Failure("PredictionApi.Connection", "Could not reach prediction service."));
        }
    }

    // --- Internal DTOs for External Contract Mapping ---

    private class ExternalGradeRecord
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int Semester { get; set; }

        // Explicitly map to snake_case to satisfy Python API Pydantic model
        [JsonPropertyName("grade_lab")]
        public double GradeLab { get; set; }

        [JsonPropertyName("grade_masterclass")]
        public double GradeMasterclass { get; set; }
    }

    private class PredictionResponse
    {
        public List<StudentPredictionDto> Predictions { get; set; } = [];
    }
}
