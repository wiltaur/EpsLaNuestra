using EpsLaNuestra.Domain.DTOs;

namespace EpsLaNuestra.Domain.Interfaces
{
    public interface IBlazorConnectUtility
    {
        Task SendEventToBlazor(PatientHistoryDto patientHistory, CancellationToken cancellationToken);
    }
}