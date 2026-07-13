using Microsoft.AspNetCore.SignalR;

namespace EpsLaNuestra.BlazorApp.Hubs;

public class AdmisionHub : Hub
{
    public async Task NotifyNewAdmission(string patientNumber, int copaymentAmount)
    {
        await Clients.All.SendAsync("ReceiveAdmission", patientNumber, copaymentAmount);
    }
}