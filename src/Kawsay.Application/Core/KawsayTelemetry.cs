using System.Diagnostics;

namespace Application.Core;

public static class KawsayTelemetry
{
    // The Service Name used in Grafana
    public const string ServiceName = "kawsay-api";
    
    // The Activity Source for manual instrumentation
    public static readonly ActivitySource ActivitySource = new(ServiceName);

    // Standard Attribute Keys for consistency
    public static class Attributes
    {
        public const string TimetableId = "kawsay.timetable.id";
        public const string JobId = "kawsay.solver.job_id";
        public const string ConstraintCount = "kawsay.solver.constraints.count";
        public const string ActivityCount = "kawsay.solver.activities.count";
        public const string GenerationStatus = "kawsay.solver.status";
        public const string QualityScore = "kawsay.solver.score";
    }
}
