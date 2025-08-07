using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PetersenTestingAppLibrary.Classes;
using PetersenTestingAppLibrary.Services;
using Microsoft.JSInterop;

namespace PetersenTestingApp.Components.Pages.MonitoringPage;

public partial class MonitoringPage : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; }
    private List<SensorReading> liveReadings = new();
    private List<SensorReading> plotData = new();
    private string? errorMessage;

    [Inject] private SensorDataService SensorDataService { get; set; }

    private IReadOnlyCollection<SensorReading> readings = [];

    protected override void OnInitialized()
    {
        liveReadings = SensorDataService.GetReadings().ToList();
        SensorDataService.OnChange += OnSensorDataChanged;
    }

    private void OnSensorDataChanged()
    {
        var updatedReadings = SensorDataService.GetReadings();

        // Create a dictionary for fast lookups
        var readingDict = liveReadings.ToDictionary(r => r.SensorID);

        // Update or insert readings
        foreach (var reading in updatedReadings)
        {
            readingDict[reading.SensorID] = reading;
        }

        liveReadings = readingDict.Values.ToList();
        InvokeAsync(StateHasChanged);
    }


    public void Dispose()
    {
        SensorDataService.OnChange -= OnSensorDataChanged;
    }


    private PlotQuery query = new()
    {
        StartDate = DateTime.UtcNow.AddHours(-12),
        EndDate = DateTime.UtcNow
    };

    protected override async Task OnInitializedAsync()
    {
        liveReadings = await DashboardBackendService.GetLatestSensorReadingsAsync();
    }
    private async Task OnQuerySubmit()
    {
        if (string.IsNullOrWhiteSpace(query.SensorId))
        {
            errorMessage = "Please enter a sensor ID.";
            return;
        }

        plotData = await DashboardBackendService
            .GetSensorReadingsForDateRangeAsync(query.SensorId, query.StartDate, query.EndDate);

        if (plotData is null || !plotData.Any())
            return;

        errorMessage = null;
        await InvokeAsync(StateHasChanged); // ensures UI re-renders
    }

    public class PlotQuery
    {
        public string SensorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
