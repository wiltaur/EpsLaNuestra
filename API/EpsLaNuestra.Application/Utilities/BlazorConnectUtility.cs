using EpsLaNuestra.Domain.DTOs;
using EpsLaNuestra.Domain.Interfaces;
using EpsLaNuestra.Domain.Persistence;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EpsLaNuestra.Application.Utilities;

public class BlazorConnectUtility : IBlazorConnectUtility
{
    private readonly ILogger<BlazorConnectUtility> _logger;
    private readonly IOptions<BlazorServerSettings> _options;

    public BlazorConnectUtility(ILogger<BlazorConnectUtility> logger, IOptions<BlazorServerSettings> options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task SendEventToBlazor(PatientHistoryDto patientHistory, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            HubConnection? connection = null;
            try
            {
                connection = new HubConnectionBuilder()
                    .WithUrl(_options.Value.Url)
                    .Build();

                await connection.StartAsync();

                await connection.InvokeAsync(
                    _options.Value.SignalNotify,
                    patientHistory.NumberId,
                    patientHistory.Name,
                    patientHistory.Copayment, 
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación a Blazor");
            }
            finally
            {
                if (connection is not null)
                {
                    await connection.StopAsync();
                    await connection.DisposeAsync();
                }
            }
        }, cancellationToken);
        await Task.CompletedTask;
    }
}