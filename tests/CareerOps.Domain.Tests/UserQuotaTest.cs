using CareerOps.Domain.Entities;
using CareerOps.Domain.Exceptions;
using FluentAssertions;

namespace CareerOps.Domain.Tests;

public class UserQuotaTest
{
    [Fact]
    public void Should_Increment_UsedRequests_When_Under_Limit()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var quota = new UserQuota(ownerId, 5);

        //act
        quota.ConsumeAnalysis();

        //assert
        quota.UsedDailyRequests.Should().Be(1);

    }

    [Fact]
    public void Should_Throw_QuotaExceededException_When_Limit_Is_Reached()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var quota = new UserQuota(ownerId, 1);
        quota.ConsumeAnalysis();

        //act
        Action act = () => quota.ConsumeAnalysis();

        //assert
        act.Should().Throw<QuotaExceededDomainException>();
    }

    [Fact]
    public void Should_Reset_UsedRequests_When_New_Day_Starts()
    {
        //Arrange
        var ownerId = Guid.NewGuid();
        var quota = new UserQuota(ownerId, 1);

        //Act
        quota.ConsumeAnalysis();
        TimeTravelToYesterday(quota);
        quota.ConsumeAnalysis();

        //Assert
        quota.UsedDailyRequests.Should().Be(1);
    }

    private void TimeTravelToYesterday(UserQuota quota)
    {
        var property = typeof(UserQuota).GetProperty("LastResetDate");
        // We force the date to be exactly 24 hours ago
        property?.SetValue(quota, DateTime.UtcNow.AddDays(-1).Date);
    }

}
