// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KawsayApiMockup.Data; // Import MockData

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy to allow frontend to connect
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin() // Be specific about origins in production
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Optional: Redirect HTTP to HTTPS

app.UseCors(); // Use the CORS policy

app.UseAuthorization();

app.MapControllers(); // Map controller routes

// Optional: Add some initial data after building the app if needed for testing
// This is already handled in the static constructor of MockData, but you could add more here
// Example: Create a timetable and a class after startup
// MockData.AddTimetable(new KawsayApiMockup.DTOs.CreateTimetableRequest { Name = "Sample Timetable", Days = new List<string> { "Monday", "Tuesday" }, Periods = new List<KawsayApiMockup.DTOs.CreateTimetablePeriodDto> { new KawsayApiMockup.DTOs.CreateTimetablePeriodDto { Start = "09:00", End = "09:30" } } });
// var sampleTimetable = MockData.Timetables.First();
// var sampleCourse = MockData.Courses.First();
// var sampleTeacher = MockData.First();
// MockData.AddClass(new KawsayApiMockup.DTOs.CreateClassRequest { TimetableId = sampleTimetable.Id, CourseId = sampleCourse.Id, TeacherId = sampleTeacher.Id, Occurrences = new List<KawsayApiMockup.DTOs.CreateClassOccurrenceDto> { new KawsayApiMockup.DTOs.CreateClassOccurrenceDto { DayId = sampleTimetable.Days.First().Id, StartPeriodId = sampleTimetable.Periods.First().Id, Length = 2 } } });


app.Run();
