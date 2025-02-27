﻿using System.Globalization;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace HASSAgent.Models.HomeAssistant.Sensors.GeneralSensors.SingleValue
{
    public class GpuLoadSensor : AbstractSingleValueSensor
    {
        private readonly IHardware _gpu;

        public GpuLoadSensor(int? updateInterval = null, string name = "GPULoad", string id = default) : base(name ?? "GPULoad", updateInterval ?? 30, id)
        {
            var computer = new Computer
            {
                IsCpuEnabled = false,
                IsGpuEnabled = true,
                IsMemoryEnabled = false,
                IsMotherboardEnabled = false,
                IsControllerEnabled = false,
                IsNetworkEnabled = false,
                IsStorageEnabled = false,
            };
            
            computer.Open();
            _gpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia);

            computer.Close();
        }

        public override DiscoveryConfigModel GetAutoDiscoveryConfig()
        {
            return AutoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel()
            {
                Name = Name,
                Unique_id = Id,
                Device = Variables.DeviceConfig,
                State_topic = $"{Variables.AppSettings.MqttDiscoveryPrefix}/{Domain}/{Variables.DeviceConfig.Name}/{ObjectId}/state",
                Unit_of_measurement = "%",
                Availability_topic = $"{Variables.AppSettings.MqttDiscoveryPrefix}/{Domain}/{Variables.DeviceConfig.Name}/availability"
            });
        }

        public override string GetState()
        {
            if (_gpu == null) return "NotSupported";

            _gpu.Update();
            var sensor = _gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);

            if (sensor == null) return "NotSupported";

            return sensor.Value.HasValue ? sensor.Value.Value.ToString("#.##", CultureInfo.InvariantCulture) : "Unknown";
        }
    }
}
