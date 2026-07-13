using EpsLaNuestra.Application.DTOs;
using EpsLaNuestra.Application.Utilities;
using EpsLaNuestra.Domain.Entities;
using EpsLaNuestra.Domain.Interfaces;
using EpsLaNuestra.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace EpsLaNuestra.Application.Features.Patients.Commands;

public class AdmitPatientCommandHandler : IRequestHandler<AdmitPatientCommand, ApiResponseUtility<bool>>
{
    private readonly IPatientMongoRepository _patientMongoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ResiliencePipeline _sqlResiliencePipeline;
    private readonly ILogger<AdmitPatientCommandHandler> _logger;
    private readonly IBlazorConnectUtility _blazorConnectUtility;

    public AdmitPatientCommandHandler(IPatientMongoRepository patientRepository, 
        IUnitOfWork unitOfWork,
        ResiliencePipelineProvider<string> pipelineProvider,
        ILogger<AdmitPatientCommandHandler> logger,
        IBlazorConnectUtility blazorConnectUtility)
    {
        _patientMongoRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _sqlResiliencePipeline = pipelineProvider.GetPipeline("sql-retry-pipeline");
        _logger = logger;
        _blazorConnectUtility = blazorConnectUtility;
    }

    public async Task<ApiResponseUtility<bool>> Handle(AdmitPatientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _patientMongoRepository.SaveResourceAsync(request.PatientHistory.ResourceType, request.PatientHistory.Id, request.RawJson);

            try
            {
                await _sqlResiliencePipeline.ExecuteAsync(async token =>
                {
                    await _unitOfWork.Copayments.SaveTransactionAsync(MapInfoPatient(request.PatientHistory), cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "El almacenamiento en SQL Server falló definitivamente tras 3 reintentos exponenciales.");
                throw;
            }

            await _blazorConnectUtility.SendEventToBlazor(request.PatientHistory.Id, request.PatientHistory.Copayment);

            return new ApiResponseUtility<bool>(true)
            {
                IsSuccess = true,
                ReturnMessage = $"{request.PatientHistory.ResourceType} con ID {request.PatientHistory.Id} guardado correctamente."
            };
        }
        catch (Exception ex)
        {
            //TODO: A futuro acá implementar el envío del mensaje con el NumeroDocumento del paciente y ValorCopago
            //para que una Azure Function la tome y ejecute el proceso de generar la Admisión
            //y así no quede en estado inconsistente...

            return new ApiResponseUtility<bool>(false)
            {
                IsSuccess = false,
                ReturnMessage = ex.InnerException == null ? ex.Message : ex.InnerException.Message
            };
        }
    }

    private static Copayment MapInfoPatient(PatientHistoryDto patientHistory)
    {
        return new()
        {
            PatientDocument = patientHistory.Id,
            CopaymentValue = patientHistory.Copayment
        };
    }
}