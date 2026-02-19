namespace CareerOps.Domain.Interfaces;
    
public interface IEntidadeAuditavel
    {
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }