namespace ApplyWise.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity) : base($"{entity} não encontrado.") { }
}
