using System;
using System.Collections.Generic;
using System.Linq;

namespace StagWare.FanControl.Configurations
{
    public class FanControlConfigV2 : ICloneable
    {
        #region Constants

        private const string AdjustFanControlModeDescription = "Set EC to manual control";
        private const string SetThermalZoneDescription = "Select thermal zone";
        private const string FakeTemperatureDescription = "Fake thermal zone temperature";

        #endregion

        #region Properties

        public string NotebookModel { get; set; }
        public int EcPollInterval { get; set; }
        public bool ReadWriteWords { get; set; }
        public int CriticalTemperature { get; set; }
        public List<FanConfiguration> FanConfigurations { get; set; }
        public List<RegisterWriteConfiguration> RegisterWriteConfigurations { get; set; }

        #endregion

        #region Constructor

        public FanControlConfigV2()
        {
            this.CriticalTemperature = 75;
            this.EcPollInterval = 3000;
            this.FanConfigurations = new List<FanConfiguration>();
            this.RegisterWriteConfigurations = new List<RegisterWriteConfiguration>();
        }

        public FanControlConfigV2(StagWare.FanControl.Configurations.FanControlConfig configV1)
            : this()
        {
            this.EcPollInterval = configV1.EcPollInterval;
            this.ReadWriteWords = configV1.ReadWriteWords;
            this.NotebookModel = configV1.NotebookModel;

            this.FanConfigurations.Add(new FanConfiguration()
            {
                WriteRegister = configV1.WriteRegister,
                ReadRegister = configV1.ReadRegister,
                MaxSpeedValue = configV1.MaxSpeedValue,
                MinSpeedValue = configV1.MinSpeedValue,
                ResetRequired = true,
                FanSpeedResetValue = configV1.FanSpeedResetValue,
                TemperatureThresholds = configV1.TemperatureThresholds
            });

            if (configV1.AdjustFanControlMode)
            {
                this.RegisterWriteConfigurations.Add(new RegisterWriteConfiguration()
                {
                    Description = AdjustFanControlModeDescription,
                    Register = configV1.EcConfigurationRegister,
                    Value = configV1.ManualControlModeValue,
                    ResetRequired = true,
                    ResetValue = configV1.AutoControlModeValue,
                    WriteMode = RegisterWriteMode.Set,
                    WriteOccasion = RegisterWriteOccasion.OnInitialization
                });
            }

            if (configV1.FakeTemperature)
            {
                foreach (int idx in configV1.ThermalZonesIndices)
                {
                    this.RegisterWriteConfigurations.Add(new RegisterWriteConfiguration()
                    {
                        Description = SetThermalZoneDescription,
                        Register = configV1.ThermalZoneRegister,
                        Value = idx,
                        ResetRequired = true,
                        ResetValue = idx,
                        WriteMode = RegisterWriteMode.Set,
                        WriteOccasion = RegisterWriteOccasion.OnInitialization
                    });

                    this.RegisterWriteConfigurations.Add(new RegisterWriteConfiguration()
                    {
                        Description = FakeTemperatureDescription,
                        Register = configV1.TemperatureRegister,
                        Value = configV1.FakeTemperatureValue,
                        ResetRequired = true,
                        ResetValue = configV1.TemperatureResetValue,
                        WriteMode = RegisterWriteMode.Set,
                        WriteOccasion = RegisterWriteOccasion.OnInitialization
                    });
                }
            }

            foreach (StagWare.FanControl.Configurations.RegisterWriteRequest request in configV1.RegisterWriteRequests)
            {
                this.RegisterWriteConfigurations.Add(new RegisterWriteConfiguration()
                {
                    Register = request.Register,
                    Value = request.Value,
                    ResetRequired = true,
                    ResetValue = request.Value,
                    WriteMode = request.WriteMode,
                    WriteOccasion = RegisterWriteOccasion.OnWriteFanSpeed
                });
            }
        }

        #endregion

        #region ICloneable implementation

        public object Clone()
        {
            return new FanControlConfigV2()
            {
                NotebookModel = this.NotebookModel,
                EcPollInterval = this.EcPollInterval,
                ReadWriteWords = this.ReadWriteWords,
                CriticalTemperature = this.CriticalTemperature,

                FanConfigurations = this.FanConfigurations
                .Select(x => x.Clone() as FanConfiguration).ToList(),

                RegisterWriteConfigurations = this.RegisterWriteConfigurations
                .Select(x => x.Clone() as RegisterWriteConfiguration).ToList()
            };
        }

        #endregion
    }
}
