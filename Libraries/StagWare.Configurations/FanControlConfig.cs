using StagWare.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StagWare.FanControl.Configurations
{
    public class FanControlConfig : ICloneable
    {
        #region Constructor

        public FanControlConfig()
        {
            NotebookModel = string.Empty;
            EcPollInterval = 3000;
            ThermalZonesIndices = new int[] { 1 };
        }

        #endregion

        #region Properties

        public string UniqueId { get; set; }

        public string NotebookModel { get; set; }
        public int EcPollInterval { get; set; }
        public bool ReadWriteWords { get; set; }

        // Basic
        public int ReadRegister { get; set; }
        public int WriteRegister { get; set; }
        public int MinSpeedValue { get; set; }
        public int MaxSpeedValue { get; set; }
        public int FanSpeedResetValue { get; set; }

        // Advanced2
        public bool AdjustFanControlMode { get; set; }
        public int EcConfigurationRegister { get; set; }
        public int ManualControlModeValue { get; set; }
        public int AutoControlModeValue { get; set; }

        // Advanced3
        public bool FakeTemperature { get; set; }
        public int TemperatureRegister { get; set; }
        public int ThermalZoneRegister { get; set; }
        public int FakeTemperatureValue { get; set; }
        public int TemperatureResetValue { get; set; }
        public int[] ThermalZonesIndices { get; set; }

        // Advanced 4
        public List<RegisterWriteRequest> RegisterWriteRequests { get; set; }

        // Thresholds
        public List<TemperatureThreshold> TemperatureThresholds { get; set; }

        #endregion

        #region ICloneable implementation
        
        public object Clone()
        {
            return new FanControlConfig()
            {
                AdjustFanControlMode = this.AdjustFanControlMode,
                AutoControlModeValue = this.AutoControlModeValue,
                EcConfigurationRegister = this.EcConfigurationRegister,
                EcPollInterval = this.EcPollInterval,
                FakeTemperature = this.FakeTemperature,
                FakeTemperatureValue = this.FakeTemperatureValue,
                FanSpeedResetValue = this.FanSpeedResetValue,
                ManualControlModeValue = this.ManualControlModeValue,
                MaxSpeedValue = this.MaxSpeedValue,
                MinSpeedValue = this.MinSpeedValue,
                NotebookModel = this.NotebookModel,
                ReadRegister = this.ReadRegister,
                ReadWriteWords = this.ReadWriteWords,
                TemperatureRegister = this.TemperatureRegister,
                TemperatureResetValue = this.TemperatureResetValue,
                ThermalZoneRegister = this.ThermalZoneRegister,
                UniqueId = this.UniqueId,
                WriteRegister = this.WriteRegister,
                ThermalZonesIndices = (int[])this.ThermalZonesIndices.Clone(),

                RegisterWriteRequests = this.RegisterWriteRequests
                    .Select(x => x.Clone() as RegisterWriteRequest).ToList(),

                TemperatureThresholds = this.TemperatureThresholds
                    .Select(x => x.Clone() as TemperatureThreshold).ToList()
            };
        }

        #endregion
    }
}
