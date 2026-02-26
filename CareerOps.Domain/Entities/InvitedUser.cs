namespace CareerOps.Domain.Entities;

public class InvitedUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
