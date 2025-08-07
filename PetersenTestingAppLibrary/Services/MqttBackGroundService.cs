using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using PetersenTestingAppLibrary.Classes;
using System.Text;
using System.Text.Json;


public class MqttBackgroundService : BackgroundService
{
    private readonly ILogger<MqttBackgroundService> _logger;
    private readonly SensorDataService _sensorDataService;
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientFactory _mqttFactory;

    public MqttBackgroundService(ILogger<MqttBackgroundService> logger, SensorDataService sensorDataService)
    {
        _logger = logger;
        _sensorDataService = sensorDataService;
        _mqttFactory = new MqttClientFactory();
        _mqttClient = _mqttFactory.CreateMqttClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("1778b10659bc42eb8a5bf9ecbd39379d.s1.eu.hivemq.cloud", 8883) // Replace with your broker address
            .WithClientId("PeteLinkDashboard")
            .WithCredentials("PeteLink", "Natomisen7") // Replace with your username and password
            .WithTlsOptions(tls => {
                tls.UseTls();
            })
            .WithCleanSession()
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                var topic = e.ApplicationMessage.Topic;

                _logger.LogInformation($"[MQTT RAW] Topic: {topic} | Payload: {payload}");
                _logger.LogInformation($"MQTT Message received. Topic: {topic} | Payload: {payload}");

                // Example: parse the payload and push to shared state
                var reading = JsonSerializer.Deserialize<SensorReading>(payload);
                if (reading != null)
                {
                    _sensorDataService.UpdateReading(reading);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MQTT message");
            }
        };

        _mqttClient.ConnectedAsync += async e =>
        {
            _logger.LogInformation("Connected to MQTT broker.");

            try
            {
                await _mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => { f.WithTopic("pete/pressure/#"); })
                    .Build());

                _logger.LogInformation("Subscribed to topic pete/pressure/#");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription failed.");
            }
        };

        _mqttClient.DisconnectedAsync += async e =>
        {
            _logger.LogWarning("Disconnected from MQTT broker. Reconnecting in 5 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            try
            {
                await _mqttClient.ConnectAsync(options, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnection attempt failed.");
            }
        };

        try
        {
            await _mqttClient.ConnectAsync(options, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initial MQTT connection failed.");
        }
    }
}