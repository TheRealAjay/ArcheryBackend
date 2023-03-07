using ArcheryBackend.Archery;
using Microsoft.EntityFrameworkCore;

namespace ArcheryBackend.Contexts;

public class ArcheryContext : DbContext
{
    private readonly IConfiguration Configuration;

    public DbSet<ArcheryEvent> Event { get; set; }
    public DbSet<Participant> Participant { get; set; }
    public DbSet<Score> Score { get; set; }
    public DbSet<Target> Target { get; set; }

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