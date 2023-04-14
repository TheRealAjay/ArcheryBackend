using ArcheryBackend.Archery;
using ArcheryBackend.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArcheryBackend.Contexts;

public class ArcheryContext : DbContext
{
    private readonly IConfiguration Configuration;

    public DbSet<ArcheryEvent> Events { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<Target> Targets { get; set; }
    public DbSet<ApplicationUser> Users { get; set; }
    public ArcheryContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to postgres with connection string from app settings
        options.UseNpgsql(Configuration["DBConnectionString"] ?? string.Empty);
    }
}