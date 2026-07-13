using EpsLaNuestra.Application.DTOs;
using EpsLaNuestra.Application.Features.Patients.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EpsLaNuestra.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Admit Patient with their medical history.
    /// </summary>
    /// <param name="rawJson">JSon FHIR/HL7 that content all information for a Patient.</param>
    /// <returns>When admit successfully, true and Ok are returned, otherwise false and InternalError are returned.</returns>
    [HttpPost]
    public async Task<IActionResult> AdmitPatient([FromBody] JsonElement rawJson)
    {
        if (rawJson.ValueKind != JsonValueKind.Object)
        {
            return BadRequest("El cuerpo de la petición debe ser un objeto JSON válido.");
        }
        if (!rawJson.TryGetProperty("resourceType", out var resourceTypeProp) ||
            !rawJson.TryGetProperty("id", out var idProp) ||
            !rawJson.TryGetProperty("paymentData", out JsonElement paymentDataElement) ||
            !paymentDataElement.TryGetProperty("copayment", out JsonElement copaymentElement))
        {
            return BadRequest("El JSON debe contener las propiedades obligatorias 'resourceType', 'id' y 'paymentData:copayment'.");
        }

        PatientHistoryDto patientHistory = new()
        {
            ResourceType = resourceTypeProp.GetString()!,
            Id = idProp.GetString()!,
            Copayment = copaymentElement.GetInt32()!
        };

        var command = new AdmitPatientCommand(patientHistory, rawJson.GetRawText());
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }
}