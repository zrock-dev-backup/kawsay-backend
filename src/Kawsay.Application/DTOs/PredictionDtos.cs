using System.Text.Json.Serialization;

namespace Application.DTOs;

public record StudentGradeInputDto(
    int StudentId,
    int CourseId,
    int Semester,
    double GradeLab,
    double GradeMasterclass
);

public enum PredictionOutcome
{
    PASS,
    FAIL
}

public record StudentPredictionDto(
    int StudentId,
    int CourseId,
    int Semester,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    PredictionOutcome PredictedOutcome,
    double Confidence,
    List<string> Drivers
);
