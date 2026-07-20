using EpsLaNuestra.Application.Features.Patients.Commands;
using EpsLaNuestra.Application.Wrappers;
using EpsLaNuestra.Domain.DTOs;
using FluentValidation;
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
    /// <param name="validator">Service Injection.</param>
    /// <returns>When admit successfully, true and Ok are returned, otherwise false and InternalError are returned.</returns>
    [HttpPost]
    public async Task<IActionResult> AdmitPatient(
        [FromBody] JsonElement rawJson,
        [FromServices] IValidator<PatientJsonWrapper> validator)
    {
        var wrapper = new PatientJsonWrapper { RawJson = rawJson };
        var validationResult = validator.Validate(wrapper);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(new { Errors = errors });
        }

        var patientDataElement = rawJson.GetProperty("patient");
        var paymentDataElement = rawJson.GetProperty("paymentData");
        
        PatientHistoryDto patientHistory = new()
        {
            ResourceType = rawJson.GetProperty("resourceType").GetString()!,
            Id = rawJson.GetProperty("id").GetString()!,
            NumberId = patientDataElement.GetProperty("numberId").GetString()!,
            Name = patientDataElement.GetProperty("name").GetString()!,
            Copayment = paymentDataElement.GetProperty("copayment").GetInt32()
        };

        var command = new AdmitPatientCommand(patientHistory, rawJson.GetRawText());
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }
}