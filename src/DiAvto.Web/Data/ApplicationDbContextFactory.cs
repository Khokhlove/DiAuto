using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DiAvto.Web.Data;

/// <summary>
/// Design-time factory used by `dotnet ef` so migrations can be generated
/// without starting the web application and its background services.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=diavto-design.db")
            .Options;

        return new ApplicationDbContext(options);
    }
}
