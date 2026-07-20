using EpsLaNuestra.Application.Wrappers;
using FluentValidation;
using System.Text.Json;

namespace EpsLaNuestra.Application.Validators;

public class PatientJsonValidator : AbstractValidator<PatientJsonWrapper>
{
    public PatientJsonValidator()
    {
        RuleFor(x => x.RawJson.ValueKind)
            .Equal(JsonValueKind.Object)
            .WithMessage("El cuerpo de la petición debe ser un objeto JSON válido.");

        RuleFor(x => x.RawJson)
            .Must(json => json.TryGetProperty("resourceType", out _))
            .WithMessage("Falta 'resourceType'.");

        RuleFor(x => x.RawJson)
            .Must(json => json.TryGetProperty("id", out _))
            .WithMessage("Falta 'id'.");

        RuleFor(x => x.RawJson)
            .Must(json => json.TryGetProperty("patient", out var patient) &&
                          patient.TryGetProperty("numberId", out _) &&
                          patient.TryGetProperty("name", out _))
            .WithMessage("Faltan propiedades en 'patient' (numberId, name).");

        RuleFor(x => x.RawJson)
            .Must(json => json.TryGetProperty("paymentData", out var payment) &&
                          payment.TryGetProperty("copayment", out _))
            .WithMessage("Faltan propiedades en 'paymentData' (copayment).");
    }
}