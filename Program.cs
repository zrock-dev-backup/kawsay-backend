// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KawsayApiMockup.Data; // Import KawsayDbContext
using Microsoft.EntityFrameworkCore; // Import EF Core namespace
using System.Linq; // Add for potential seeding logic

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with SQLite
builder.Services.AddDbContext<KawsayDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);


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

// Apply database migrations on startup (for development)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KawsayDbContext>();
    dbContext.Database.Migrate(); // Apply any pending migrations
    // Optional: Add code here to check if seed data exists and add it if not
    // Example:
    // if (!dbContext.Courses.Any())
    // {
    //     dbContext.Courses.AddRange(
    //         new KawsayApiMockup.Entities.CourseEntity { Id = 1, Name = "Programming 1", Code = "CSPR-101" },
    //         ...
    //     );
    //     dbContext.SaveChanges();
    // }
    // This seed data is now in OnModelCreating, which is applied by Migrate()
}


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

app.Run();
