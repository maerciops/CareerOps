namespace CareerOps.Domain.Exceptions;

public class QuotaExceededDomainException: Exception
{
    public QuotaExceededDomainException(string message): base (message) { }
}
