using EpsLaNuestra.Application.DTOs;
using EpsLaNuestra.Application.Utilities;
using MediatR;

namespace EpsLaNuestra.Application.Features.Patients.Commands;

public record AdmitPatientCommand(PatientHistoryDto PatientHistory, object RawJson) : IRequest<ApiResponseUtility<bool>>;