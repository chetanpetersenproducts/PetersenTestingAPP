using Microsoft.Data.SqlClient;
using PetersenTestingAppLibrary.Classes;

namespace PetersenTestingAppLibrary.Services;

public class DashboardBackendService
{
    private readonly Utils _utils;

    public DashboardBackendService(Utils utils)
    {
        _utils = utils;
    }

    public async Task<List<SensorReading>> GetLatestSensorReadingsAsync()
    {
        var results = new List<SensorReading>();

        var query = @"
            WITH LatestReadings AS (
                SELECT *,
                       ROW_NUMBER() OVER (PARTITION BY SensorID ORDER BY TimeStamp DESC) AS rn
                FROM PeteLinkTest
                WHERE TimeStamp >= DATEADD(HOUR, -12, GETUTCDATE())
            )
            SELECT SensorID, TimeStamp, PressurePSI, BatteryVoltage, Temperature
            FROM LatestReadings
            WHERE rn = 1
        ";

        using var conn = new SqlConnection(_utils.sqlServerConnection);
        using var cmd = new SqlCommand(query, conn);
        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new SensorReading
            {
                SensorID = reader["SensorID"].ToString(),
                TimeStamp = Convert.ToDateTime(reader["TimeStamp"]),
                PressurePSI = Convert.ToDouble(reader["PressurePSI"]),
                BatteryVoltage = Convert.ToDouble(reader["BatteryVoltage"]),
                Temperature = Convert.ToDouble(reader["Temperature"]),
            });
        }

        return results;
    }
    public async Task<List<SensorReading>> GetSensorReadingsForDateRangeAsync(string sensorId, DateTime start, DateTime end)
    {
        var results = new List<SensorReading>();

        var query = @"
        SELECT SensorID, TimeStamp, PressurePSI, BatteryVoltage, Temperature
        FROM PeteLinkTest
        WHERE SensorID = @SensorID
          AND TimeStamp BETWEEN @Start AND @End
        ORDER BY TimeStamp";

        using var conn = new SqlConnection(_utils.sqlServerConnection);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@SensorID", sensorId);
        cmd.Parameters.AddWithValue("@Start", start);
        cmd.Parameters.AddWithValue("@End", end);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new SensorReading
            {
                SensorID = reader["SensorID"].ToString(),
                TimeStamp = Convert.ToDateTime(reader["TimeStamp"]),
                PressurePSI = Convert.ToDouble(reader["PressurePSI"]),
                BatteryVoltage = Convert.ToDouble(reader["BatteryVoltage"]),
                Temperature = Convert.ToDouble(reader["Temperature"]),
            });
        }

        return results;
    }

}
