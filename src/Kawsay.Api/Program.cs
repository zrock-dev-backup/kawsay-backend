using Application.Features.Scheduling;
using Application.Interfaces.Persistence;
using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var envName = builder.Environment.EnvironmentName;
var contentRoot = builder.Environment.ContentRootPath;
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[DEBUG] Environment Name: '{envName}'");
Console.WriteLine($"[DEBUG] Content Root Path: '{contentRoot}'");
Console.WriteLine($"[DEBUG] Connection String: '{connStr}'");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<KawsayDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(KawsayDbContext).Assembly.FullName)
    );
});

builder.Services.AddScoped<SchedulingService>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();
builder.Services.AddScoped<IClassOccurrenceRepository, ClassOccurrenceRepository>();

builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<TimetableService>();
builder.Services.AddScoped<ClassService>();
builder.Services.AddScoped<CalendarizationService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
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

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();