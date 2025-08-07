using PetersenTestingAppLibrary.Classes;
using System.Collections.Concurrent;

public class SensorDataService
{
    private readonly ConcurrentDictionary<string, SensorReading> _latest = new();
    public event Action? OnChange;

    public IReadOnlyCollection<SensorReading> GetReadings() => _latest.Values.ToList();

    public void UpdateReading(SensorReading reading)
    {
        _latest[reading.SensorID] = reading;
        OnChange?.Invoke();
    }
}

