using System.Text.Json.Serialization;
using Api.Converters;
using Api.Middleware;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.Services;
using Infrastructure.External;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Protos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var envName = builder.Environment.EnvironmentName;
var contentRoot = builder.Environment.ContentRootPath;
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[DEBUG] Environment Name: '{envName}'");
Console.WriteLine($"[DEBUG] Content Root Path: '{contentRoot}'");
Console.WriteLine($"[DEBUG] Connection String: '{connStr}'");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<KawsayDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(KawsayDbContext).Assembly.FullName)
    );
});
builder.Services.AddScoped<ICourseRequirementRepository, CourseRequirementRepository>();
// builder.Services.AddScoped<IAvailabilityReadModelRepository, AvailabilityReadModelRepository>();
builder.Services.AddScoped<IAcademicStructureRepository, AcademicStructureRepository>();
builder.Services.AddScoped<IClassOccurrenceRepository, ClassOccurrenceRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IClassTypeConfigurationRepository, ClassTypeConfigurationRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IStudentModuleGradeRepository, StudentModuleGradeRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherQualificationRepository, TeacherQualificationRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();

builder.Services.AddScoped<AcademicStructureService>();
builder.Services.AddScoped<CalendarizationService>();
builder.Services.AddScoped<ClassService>();
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<EndofModuleService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<TimetableService>();
builder.Services.AddScoped<ICourseRequirementService, CourseRequirementService>();
// builder.Services.AddScoped<ISchedulingEngineService, SchedulingEngineService>();
builder.Services.AddScoped<ITimetableAssignmentRepository, TimetableAssignmentRepository>();
builder.Services.AddScoped<ITimetableAssignmentService, TimetableAssignmentService>();
builder.Services.AddScoped<RosterSyncService>();
builder.Services.AddScoped<IStagedPlacementRepository, StagedPlacementRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<SchedulingService>();
builder.Services.AddScoped<IStudentIssueRepository, StudentIssueRepository>();
builder.Services.AddScoped<IStudentAuditService, StudentAuditService>();

// gRPC implementation
builder.Services.AddScoped<ISolverClient, SolverGrpcClient>();
builder.Services.AddScoped<TimetableGenerationService>();

builder.Services.AddTransient<AcademicAuthHandler>(_ =>
    new AcademicAuthHandler(
        builder.Configuration["AcademicApi:ServiceAccountUser"] ?? "admin",
        builder.Configuration["AcademicApi:ServiceAccountRole"] ?? "admin"));

builder.Services.AddHttpClient<IAcademicApiClient, AcademicApiClient>(client =>
    {
        var baseUrl = builder.Configuration["ExternalServices:mockSIS"]
                      ?? throw new InvalidOperationException("ExternalServices:BaseUrl is missing.");
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<AcademicAuthHandler>();

builder.Services.AddHttpClient<IPredictionService, PredictionApiClient>(client =>
{
    var baseUrl = builder.Configuration["ExternalServices:PredictionApi"]
                  ?? throw new InvalidOperationException("ExternalServices:BaseUrl is missing.");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<Api.Filters.ValidationFilterAttribute>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
});

builder.Services.AddGrpcClient<TimetablingService.TimetablingServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["ExternalServices:SolverApi"] ?? throw new InvalidOperationException());
});


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KawsayDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandler>();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();

