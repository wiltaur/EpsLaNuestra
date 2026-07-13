namespace EpsLaNuestra.Domain.Interfaces
{
    public interface IBlazorConnectUtility
    {
        Task SendEventToBlazor(string patientNumber, int copaymentAmount);
    }
}