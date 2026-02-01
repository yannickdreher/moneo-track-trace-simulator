using System.ComponentModel.DataAnnotations;

namespace App.Settings
{
    public class MqttSettings
    {
        public const string SectionName = "Mqtt";

        [Required]
        [MinLength(1)]
        public string Host { get; set; } = "localhost";

        [Range(1, 65535)]
        public int Port { get; set; } = 1883;

        public string? Username { get; set; }
        public string? Password { get; set; }

        [Required]
        public string Topic { get; set; } = "factory/demo";

        public bool UseTls { get; set; } = false;

        [Range(1, 300)]
        public int KeepAlivePeriodSeconds { get; set; } = 5;

        [Range(1, 60)]
        public int TimeoutSeconds { get; set; } = 5;
    }
}