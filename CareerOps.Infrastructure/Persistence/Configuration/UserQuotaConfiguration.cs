using CareerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerOps.Infrastructure.Persistence.Configuration;

public class UserQuotaConfiguration : IEntityTypeConfiguration<UserQuota>
{
    public void Configure(EntityTypeBuilder<UserQuota> builder)
    {
        builder.ToTable("UserQuotas");

        builder.HasKey(x => x.OwnerId);
        
        builder.Property(x => x.MaxDailyRequests).IsRequired();
        builder.Property(x => x.UsedDailyRequests).IsRequired();
        builder.Property(x => x.LastResetDate).IsRequired();
    }
}
