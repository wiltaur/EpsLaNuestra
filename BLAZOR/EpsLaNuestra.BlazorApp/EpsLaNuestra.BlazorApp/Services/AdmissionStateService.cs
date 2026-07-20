using EpsLaNuestra.BlazorApp.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace EpsLaNuestra.BlazorApp.Services;

public class AdmissionStateService : IAsyncDisposable
{
    private readonly NavigationManager _navigation;
    private HubConnection? _hubConnection;
    public List<AdmissionModel> admissions = [];

    public event Func<Task>? OnStateChanged;

    public AdmissionStateService(NavigationManager navigation)
    {
        _navigation = navigation;
    }

    public async Task StartAsync()
    {
        // Evita duplicar la conexión si el servicio ya está corriendo
        if (_hubConnection is not null) return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri("/admisionHub"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string, string, int>("ReceiveAdmission", async (patientNumber, patientName, copaymentAmount) =>
        {
            // Buscar si ya existe el paciente
            var patientExist = admissions.FirstOrDefault(a => a.NumberId == patientNumber);

            if (patientExist != null)
            {
                // Si ya existe, se actualizan los valores requeridos
                patientExist.Copayment = copaymentAmount;
                patientExist.RegisterDate = DateTime.Now;
                patientExist.IsUpdate = true;
                patientExist.JustUpdated = true;

                // Se quita el efecto visual de "Actualizado" tras 3 segundos de manera segura
                _ = RemoveHighlightEffectAsync(patientExist);
            }
            else
            {
                admissions.Insert(0, new AdmissionModel
                {
                    NumberId = patientNumber,
                    Name = patientName,
                    Copayment = copaymentAmount,
                    RegisterDate = DateTime.Now
                });
            }

            // Notifica a cualquier componente que esté escuchando que hay nuevos datos disponibles
            if (OnStateChanged is not null)
                await OnStateChanged.Invoke();
        });

        await _hubConnection.StartAsync();
    }

    private async Task RemoveHighlightEffectAsync(AdmissionModel modelo)
    {
        await Task.Delay(3000);
        modelo.JustUpdated = false;

        if (OnStateChanged is not null)
        {
            await OnStateChanged.Invoke();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            _hubConnection.Remove("ReceiveAdmission");
            await _hubConnection.DisposeAsync();
        }
    }
}