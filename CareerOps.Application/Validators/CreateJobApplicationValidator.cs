using CareerOps.Application.DTOs;
using FluentValidation;

namespace CareerOps.Application.Validators;

public class CreateJobApplicationValidator: AbstractValidator<JobApplicationRequest>
{
    public CreateJobApplicationValidator()
    {
        RuleFor(field => field.Company).NotEmpty().WithMessage("O nome da empresa é obrigatório.").MaximumLength(100).WithMessage("O máximo de caracteres para o campo empresa é de 100.");
        RuleFor(field => field.JobTitle).NotEmpty().WithMessage("O nome da vaga é obrigatório.").MaximumLength(100).WithMessage("O máximo de caracteres para o campo título é de 100.");
        RuleFor(field => field.Description).NotEmpty().WithMessage("A descrição da vaga é obrigatória.");
        RuleFor(field => field.SalaryRange).GreaterThan(0).WithMessage("O valor do salário não pode ser negativo.");
        RuleFor(field => field.URL).Must(LinkValido).WithMessage("URL da vaga inválida.");
    }

    private bool LinkValido(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
               && uriResult.Host.Contains(".");
    }
}
