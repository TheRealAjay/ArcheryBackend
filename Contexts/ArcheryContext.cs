using ArcheryBackend.Archery;
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
    public DbSet<IdentityUser> Users { get; set; }

    public ArcheryContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to postgres with connection string from app settings
        options.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=xs5a3p");
    }
}