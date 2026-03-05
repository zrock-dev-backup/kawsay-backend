using System.Text.Json.Serialization;
using Api.Converters;
using Api.Middleware;
using Application.Core;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Persistence;
using Application.Services;
using Infrastructure.External;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Protos;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
var envName = builder.Environment.EnvironmentName;
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(KawsayTelemetry.ServiceName)
    .AddTelemetrySdk();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<KawsayDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(KawsayDbContext).Assembly.FullName)
    );
});
// Register Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();
builder.Services.AddScoped<IRelationalMappingRepository, RelationalMappingRepository>();
builder.Services.AddScoped<ICourseRequirementRepository, RelationalMappingRepository.CourseRequirementRepository>();
builder.Services.AddScoped<IStagedPlacementRepository, RelationalMappingRepository.StagedPlacementRepository>();
// Register Services
builder.Services.AddScoped<Stage0ConfigurationService>();
builder.Services.AddScoped<Stage1RelationalMappingService>();
builder.Services.AddScoped<Stage2ActivityService>();
builder.Services.AddScoped<TimetableGenerationService>();
builder.Services.AddScoped<ISolverClient, SolverGrpcClient>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddControllers(options => { options.Filters.Add<Api.Filters.ValidationFilterAttribute>(); })
    .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });
builder.Services.AddGrpcClient<TimetablingService.TimetablingServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["ExternalServices:SolverApi"] ??
                              throw new InvalidOperationException());
});
builder.Services.AddOpenTelemetry().WithTracing(tracing =>
{
    tracing.SetResourceBuilder(resourceBuilder)
        .AddSource(KawsayTelemetry.ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddGrpcClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true)
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["Telemetry:OtlpEndpoint"]!));
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