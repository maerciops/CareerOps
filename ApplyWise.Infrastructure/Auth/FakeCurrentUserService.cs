using ApplyWise.Domain.Interfaces;

namespace ApplyWise.Infrastructure.Auth;

public class FakeCurrentUserService : ICurrentUserService
{
    public Guid UserId => Guid.Parse("3dd4204e-0398-4b00-a168-279b01eeba83");

    public string Email => "maercio10@gmail.com";

    public bool IsAuthenticated => true;
}
