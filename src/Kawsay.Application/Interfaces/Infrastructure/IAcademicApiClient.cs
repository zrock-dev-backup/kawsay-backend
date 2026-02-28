using Application.Models.External;

namespace Application.Interfaces.Infrastructure;

public interface IAcademicApiClient
{
    Task<CurriculumCatalog?> GetCatalogAsync(CancellationToken cancellationToken = default);
    Task<Course?> GetCourseAsync(string code, CancellationToken cancellationToken = default);
    Task<Cohort?> GetCohortAsync(string id, CancellationToken cancellationToken = default);
    Task<Student?> GetStudentProfileAsync(string id, CancellationToken cancellationToken = default);
}
