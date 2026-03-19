using CareerOps.Domain.Common;
using CareerOps.Domain.Entities;
using CareerOps.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CareerOps.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public DbSet<JobApplication> Jobs { get; set; }
    public DbSet<JobAnalysis> JobAnalysis { get; set; }
    public DbSet<UserQuota> UserQuotas { get; set; }
    public DbSet<InvitedUser> InvitedUser { get; set; }

    // Propriedade auxiliar para os filtros de consulta
    public Guid CurrentUserId => _currentUserService.UserId;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;

                    if (entry.Entity.OwnerId == Guid.Empty && userId != Guid.Empty)
                    {
                        entry.Entity.OwnerId = userId;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Deleted:
                    if (entry.Metadata.FindProperty("IsDeleted") != null)
                    {
                        entry.State = EntityState.Modified;
                        entry.Property("IsDeleted").CurrentValue = true;
                        entry.Entity.UpdatedAt = now;
                    }
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<JobApplication>()
            .Property(j => j.SalaryRange)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<InvitedUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Filtra apenas entidades que herdam de BaseEntity
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<bool>("IsDeleted")
                    .HasDefaultValue(false);

                modelBuilder.Entity(entityType.ClrType).HasIndex("OwnerId");

                var filter = CreateCombinedQueryFilter(entityType.ClrType);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    private LambdaExpression CreateCombinedQueryFilter(Type type)
    {
        var parameter = Expression.Parameter(type, "e");

        var isDeletedProp = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            new[] { typeof(bool) },
            parameter,
            Expression.Constant("IsDeleted"));
        var isNotDeleted = Expression.Equal(isDeletedProp, Expression.Constant(false));

        var ownerIdProp = Expression.Property(parameter, nameof(BaseEntity.OwnerId));

        var dbContextProp = Expression.Property(Expression.Constant(this), nameof(CurrentUserId));
        var isOwner = Expression.Equal(ownerIdProp, dbContextProp);

        var combined = Expression.AndAlso(isNotDeleted, isOwner);

        return Expression.Lambda(combined, parameter);
    }
}