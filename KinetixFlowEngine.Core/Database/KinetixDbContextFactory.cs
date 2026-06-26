using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KinetixFlowEngine.Core.Database;

public class KinetixDbContextFactory
    : IDesignTimeDbContextFactory<KinetixDbContext>
{
    public KinetixDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<KinetixDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=kinetixLLM;Username=postgres;Password=Monday@01");

        return new KinetixDbContext(optionsBuilder.Options);
    }
}