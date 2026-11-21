using DiAvto.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DiAvto.Infrastructure;

public sealed class DiAvtoDbContextFactory : IDesignTimeDbContextFactory<DiAvtoDbContext>
{
    public DiAvtoDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DiAvtoDbContext>()
            .UseSqlite("Data Source=Data/diavto.db;Cache=Shared")
            .Options;

        return new DiAvtoDbContext(options);
    }
}
