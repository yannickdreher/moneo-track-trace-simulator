namespace App.Models
{
    public class StationModel
    {
        public string Name { get; set; } = string.Empty;
        public string TopicSuffix { get; set; } = string.Empty;
        public int TelemetryIntervalSeconds { get; set; } = 1;
    }
}