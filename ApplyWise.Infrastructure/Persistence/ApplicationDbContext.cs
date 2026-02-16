using ApplyWise.Domain.Entities;
using ApplyWise.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApplyWise.Infrastructure.Persistence;

public interface IEntidadeAuditavel
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

public class ApplicationDbContext: DbContext
{
    private readonly ICurrentUserService _userContext;
    public DbSet<JobApplication> Jobs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _userContext = currentUserService;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entradas = ChangeTracker.Entries<IEntidadeAuditavel>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entrada in entradas)
        {
            if (entrada.State == EntityState.Added)
            {
                entrada.Entity.CreatedAt = DateTime.UtcNow;
            }

            entrada.Entity.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
        {
            // Se a entidade tem a Shadow Property "IsDeleted"
            if (entry.Metadata.FindProperty("IsDeleted") != null)
            {
                entry.State = EntityState.Modified; // Muda de Deletar para Editar
                entry.CurrentValues["IsDeleted"] = true; // Seta o flag
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobApplication>().Property<bool>("IsDeleted").HasDefaultValue(false);
        modelBuilder.Entity<JobApplication>().HasQueryFilter(p => EF.Property<bool>(p, "IsDeleted") == false && p.OwnerId == _userContext.UserId);
        base.OnModelCreating(modelBuilder);
    }
}