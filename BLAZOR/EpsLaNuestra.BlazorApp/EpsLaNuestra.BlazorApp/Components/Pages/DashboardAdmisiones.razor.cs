using EpsLaNuestra.BlazorApp.Models;
using EpsLaNuestra.BlazorApp.Services;
using Microsoft.AspNetCore.Components;

namespace EpsLaNuestra.BlazorApp.Components.Pages;

public partial class DashboardAdmisiones : ComponentBase, IAsyncDisposable
{
    [Inject]
    protected AdmissionStateService StateService { get; set; } = default!;

    protected string DataSearch { get; set; } = string.Empty;

    protected IEnumerable<AdmissionModel> FilteredAdmissions =>
        string.IsNullOrWhiteSpace(DataSearch)
            ? StateService.admissions
            : StateService.admissions.Where(a =>
                a.NumberId.Contains(DataSearch, StringComparison.OrdinalIgnoreCase) ||
                a.Name.Contains(DataSearch, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        StateService.OnStateChanged += HandleStateChangeAsync;

        // Inicia servicio de SignalR.
        await StateService.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        StateService.OnStateChanged -= HandleStateChangeAsync;

        await Task.CompletedTask;
    }

    private async Task HandleStateChangeAsync() => await InvokeAsync(StateHasChanged);

    private void CleanFilter() => DataSearch = string.Empty;
}