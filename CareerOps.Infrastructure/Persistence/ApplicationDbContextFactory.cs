using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CareerOps.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // 1. Localizar o appsettings no projeto da API
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../CareerOps.API"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);

        // 2. Passar um "Mock" do CurrentUserService porque em tempo de migration não há usuário logado
        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeUserService());
    }
}

// Classe auxiliar simples apenas para a Factory
public class DesignTimeUserService : Domain.Interfaces.ICurrentUserService
{
    public Guid UserId => Guid.Empty;
    public string Email => "design-time@careerops.com";
    public bool IsAuthenticated => false;
}