using Microsoft.AspNetCore.SignalR;

namespace EpsLaNuestra.BlazorApp.Hubs;

public class AdmisionHub : Hub
{
    public async Task NotifyNewAdmission(string patientNumber, string patientName, int copaymentAmount)
    {
        await Clients.All.SendAsync("ReceiveAdmission", patientNumber, patientName, copaymentAmount);
    }
}