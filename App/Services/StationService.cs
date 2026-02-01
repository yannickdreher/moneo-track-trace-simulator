using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using App.Models;

namespace App.Services
{
    public partial class StationService(MqttService mqttService) : IDisposable
    {
        private readonly MqttService _mqttService = mqttService;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = false };
        private readonly CancellationTokenSource _cts = new();

        public async Task PublishEntryAsync(string[] carriers, string baseTopic, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            var carriersJson = JsonSerializer.Serialize(carriers);

            var message = new
            {
                identifier = "Entry",
                unit = "",
                values = new[]
                {
                    new
                    {
                        value = carriersJson,
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }
                }
            };

            await PublishMessageAsync(message, baseTopic, cancellationToken);
        }

        public async Task PublishExitAsync(string[] carriers, string baseTopic, string stationName, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            var carriersJson = JsonSerializer.Serialize(carriers);

            var message = new
            {
                identifier = "Exit",
                unit = "",
                values = new[]
                {
                    new
                    {
                        value = carriersJson,
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        station = stationName
                    }
                }
            };

            await PublishMessageAsync(message, baseTopic, cancellationToken);
        }

        public async Task PublishTelemetryAsync(TelemetryDataPoint dataPoint, double value, string baseTopic, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            var message = new
            {
                identifier = dataPoint.Identifier,
                unit = dataPoint.Unit,
                values = new[]
                {
                    new
                    {
                        value,
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }
                }
            };

            await PublishMessageAsync(message, baseTopic, cancellationToken);
        }

        public async Task PublishAttributeAsync(AttributeDataPoint attribute, string baseTopic, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            var message = new
            {
                identifier = attribute.Identifier,
                unit = "",
                values = new[]
                {
                    new
                    {
                        value = attribute.Value,
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }
                }
            };

            await PublishMessageAsync(message, baseTopic, cancellationToken);
        }

        public async Task PublishStateAsync(string state, string baseTopic, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            var message = new
            {
                identifier = "State",
                unit = "",
                values = new[]
                {
                    new
                    {
                        value = state,
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }
                }
            };

            await PublishMessageAsync(message, baseTopic, cancellationToken);
        }

        private async Task PublishMessageAsync(object payload, string topic, CancellationToken cancellationToken)
        {
            var jsonPayload = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
            await _mqttService.PublishAsync(topic, jsonPayload, cancellationToken);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}