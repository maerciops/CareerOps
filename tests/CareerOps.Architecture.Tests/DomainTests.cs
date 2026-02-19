using NetArchTest.Rules;
using CareerOps.Domain.Entities;

namespace CareerOps.Architecture.Tests;

public class DomainTests
{
    [Fact]
    public void Domain_Should_Not_Have_Dependency_On_Other_Layers()
    {
        var assembly = typeof(JobApplication).Assembly;

        var prohibitedLayers = new[]
        {
            "CareerOps.Application",
            "CareerOps.Infrastructure",
            "CareerOps.API"
        };

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(prohibitedLayers)
            .GetResult();

        Assert.True(result.IsSuccessful, "O Domain não pode depender de outras camadas!");
    }
}
