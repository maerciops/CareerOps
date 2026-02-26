using CareerOps.Domain.Common;
using CareerOps.Domain.Entities;
using CareerOps.Domain.Interfaces;
using CareerOps.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CareerOps.Infrastructure.Persistence;

public class ApplicationDbContext: DbContext
{
    private readonly ICurrentUserService _currentUserService;
    public DbSet<JobApplication> Jobs { get; set; }
    public DbSet<UserQuota> UserQuotas { get; set; }
    public DbSet<InvitedUser> InvitedUser { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
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

            if (entrada.State == EntityState.Modified && entrada.Entity.CreatedAt == DateTime.MinValue)
            {
                entrada.Entity.CreatedAt = DateTime.UtcNow;
            }

            entrada.Entity.UpdatedAt = DateTime.UtcNow;

            entrada.Property(p => p.UpdatedAt).IsModified = true;
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

        var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity.OwnerId == Guid.Empty)
            {
                entry.Entity.OwnerId = _currentUserService.UserId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobApplication>().Property<bool>("IsDeleted").HasDefaultValue(false);
        modelBuilder.Entity<JobApplication>().HasQueryFilter(p => EF.Property<bool>(p, "IsDeleted") == false && p.OwnerId == _currentUserService.UserId);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InvitedUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Verifica se a classe herda de BaseEntity (ajuste o nome se for diferente)
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasIndex("OwnerId"); // Índice automático para performance

                // Define o filtro global usando o ICurrentUserService
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }
    }

    private LambdaExpression ConvertFilterExpression(Type type)
    {
        var parameter = Expression.Parameter(type, "it");
        var property = Expression.Property(parameter, "OwnerId");
        var userId = Expression.Property(Expression.Constant(_currentUserService), nameof(_currentUserService.UserId));
        var comparison = Expression.Equal(property, userId);
        return Expression.Lambda(comparison, parameter);
    }
}