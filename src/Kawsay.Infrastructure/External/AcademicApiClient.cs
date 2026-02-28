using System.Net.Http.Json;
using Application.Interfaces.Infrastructure;
using Application.Models.External;

namespace Infrastructure.External;

public class AcademicApiClient : IAcademicApiClient
{
    private readonly HttpClient _httpClient;

    public AcademicApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurriculumCatalog?> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<CurriculumCatalog>(
            "/api/v1/catalog", 
            cancellationToken);
    }

    public async Task<Course?> GetCourseAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<Course>(
            $"/api/v1/catalog/courses/{Uri.EscapeDataString(code)}", 
            cancellationToken);
    }

    public async Task<Cohort?> GetCohortAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/v1/cohorts/{Uri.EscapeDataString(id)}", cancellationToken);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Cohort>(cancellationToken: cancellationToken);
    }

    public async Task<Student?> GetStudentProfileAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<Student>(
            $"/api/v1/students/{Uri.EscapeDataString(id)}", 
            cancellationToken);
    }
}
