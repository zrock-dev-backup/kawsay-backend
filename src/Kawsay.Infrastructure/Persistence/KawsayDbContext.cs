using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class KawsayDbContext(DbContextOptions<KawsayDbContext> options) : DbContext(options)
{
    // Create db tables

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}