using EpsLaNuestra.Application.Utilities;
using EpsLaNuestra.Domain.DTOs;
using MediatR;

namespace EpsLaNuestra.Application.Features.Patients.Commands;

public record AdmitPatientCommand(PatientHistoryDto PatientHistory, object RawJson) : IRequest<ApiResponseUtility<bool>>;