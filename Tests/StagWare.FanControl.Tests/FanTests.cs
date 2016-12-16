using FakeItEasy;
using StagWare.FanControl.Configurations;
using StagWare.FanControl.Plugins;
using System;
using System.Collections.Generic;
using Xunit;

namespace StagWare.FanControl.Tests
{
    public class FanTests
    {
        public class SetTargetSpeed
        {
            [Theory]
            [InlineData(0)]
            [InlineData(66.66)]
            [InlineData(100)]
            public void CallsWriteByte(float speed)
            {
                var ec = A.Fake<IEmbeddedController>();
                var cfg = new FanConfiguration()
                {
                    WriteRegister = 123,
                    MinSpeedValue = 0,
                    MaxSpeedValue = 200
                };

                var fan = new Fan(ec, cfg, 100, false);
                fan.SetTargetSpeed(speed, 0, false);

                byte expectedValue = (byte)Math.Round((cfg.MaxSpeedValue * speed) / 100);

                A.CallTo(() => ec.WriteByte((byte)cfg.WriteRegister, expectedValue))
                    .MustHaveHappened();
                Assert.Equal(speed, fan.TargetSpeed, 10);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(66.66)]
            [InlineData(100)]
            public void CallsWriteWord(float speed)
            {
                var ec = A.Fake<IEmbeddedController>();
                var cfg = new FanConfiguration()
                {
                    WriteRegister = 123,
                    MinSpeedValue = 0,
                    MaxSpeedValue = 20000
                };

                var fan = new Fan(ec, cfg, 100, true);
                fan.SetTargetSpeed(speed, 50, false);

                ushort expectedValue = (ushort)Math.Round((cfg.MaxSpeedValue * speed) / 100);

                A.CallTo(() => ec.WriteWord((byte)cfg.WriteRegister, expectedValue))
                    .MustHaveHappened();
                Assert.Equal(speed, fan.TargetSpeed, 10);
            }

            [Fact]
            public void HandlesCriticalTemperature()
            {
                var ec = A.Fake<IEmbeddedController>();
                var cfg = new FanConfiguration()
                {
                    WriteRegister = 123,
                    MinSpeedValue = 0,
                    MaxSpeedValue = 200
                };

                int criticalTemperature = 70;

                var fan = new Fan(ec, cfg, criticalTemperature, false);
                fan.SetTargetSpeed(50, criticalTemperature + 1, false);

                A.CallTo(() => ec.WriteByte((byte)cfg.WriteRegister, (byte)cfg.MaxSpeedValue))
                    .MustHaveHappened();

                Assert.True(fan.CriticalModeEnabled, nameof(fan.CriticalModeEnabled));
                Assert.Equal(100, fan.TargetSpeed, 10);
            }            

            [Fact]
            public void DoesRespectReadOnlyArg()
            {
                var ec = A.Fake<IEmbeddedController>();
                var cfg = new FanConfiguration()
                {
                    WriteRegister = 123,
                    MinSpeedValue = 0,
                    MaxSpeedValue = 200
                };

                var fan1 = new Fan(ec, cfg, 100, false);
                fan1.SetTargetSpeed(100, 50, true);

                var fan2 = new Fan(ec, cfg, 100, true);
                fan2.SetTargetSpeed(100, 50, true);

                A.CallTo(() => ec.WriteByte((byte)cfg.WriteRegister, (byte)cfg.MaxSpeedValue))
                    .MustNotHaveHappened();
                A.CallTo(() => ec.WriteWord((byte)cfg.WriteRegister, (byte)cfg.MaxSpeedValue))
                    .MustNotHaveHappened();
            }

            [Fact]
            public void HandlesAutoControl()
            {
                var ec = A.Fake<IEmbeddedController>();
                var cfg = new FanConfiguration()
                {
                    WriteRegister = 123,
                    MinSpeedValue = 0,
                    MaxSpeedValue = 200
                };

                var fan = new Fan(ec, cfg, 100, false);
                fan.SetTargetSpeed(Fan.AutoFanSpeed, 50, false);

                Assert.True(fan.AutoControlEnabled, nameof(fan.AutoControlEnabled));
            }

            [Fact]
            public void AppliesFanSpeedOverrides()
            {
                var ec = A.Fake<IEmbeddedController>();
                var fanSpeedOverride = new FanSpeedPercentageOverride()
                {
                    FanSpeedPercentage = 0,
                    FanSpeedValue = 255,
                    TargetOperation = OverrideTargetOperation.Write
                };

                var cfg = new FanConfiguration()
                {
                    WriteRegister = 123,
                    MinSpeedValue = 0,
                    MaxSpeedValue = 200,
                    FanSpeedPercentageOverrides = new List<FanSpeedPercentageOverride>()
                    {
                        fanSpeedOverride
                    }
                };

                var fan = new Fan(ec, cfg, 100, false);
                fan.SetTargetSpeed(fanSpeedOverride.FanSpeedPercentage, 50, false);

                A.CallTo(() => ec.WriteByte((byte)cfg.WriteRegister, (byte)fanSpeedOverride.FanSpeedValue))
                    .MustHaveHappened();

                Assert.Equal(fanSpeedOverride.FanSpeedPercentage, fan.TargetSpeed);
            }
        }

        [Fact]
        public void AppliesFanSpeedOverridesWhenTempIsCritical()
        {
            var ec = A.Fake<IEmbeddedController>();
            var fanSpeedOverride = new FanSpeedPercentageOverride()
            {
                FanSpeedPercentage = 100,
                FanSpeedValue = 255,
                TargetOperation = OverrideTargetOperation.Write
            };

            var cfg = new FanConfiguration()
            {
                WriteRegister = 123,
                MinSpeedValue = 0,
                MaxSpeedValue = 200,
                FanSpeedPercentageOverrides = new List<FanSpeedPercentageOverride>()
                {
                    fanSpeedOverride
                }
            };            

            int criticalTemperature = 70;

            var fan = new Fan(ec, cfg, criticalTemperature, false);
            fan.SetTargetSpeed(50, criticalTemperature + 1, false);

            A.CallTo(() => ec.WriteByte((byte)cfg.WriteRegister, (byte)fanSpeedOverride.FanSpeedValue))
                .MustHaveHappened();

            Assert.True(fan.CriticalModeEnabled, nameof(fan.CriticalModeEnabled));
            Assert.Equal(100, fan.TargetSpeed, 10);
        }

        public class GetCurrentSpeed
        {
            [Fact]
            public void CallsReadByte()
            {
                var cfg = new FanConfiguration()
                {
                    ReadRegister = 123,
                    MinSpeedValue = 0,
                    MaxSpeedValue = 200
                };

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);
                A.CallTo(() => ec.ReadByte((byte)cfg.ReadRegister)).Returns((byte)cfg.MaxSpeedValue);

                var fan = new Fan(ec, cfg, 100, false);
                fan.GetCurrentSpeed();

                A.CallTo(() => ec.ReadByte((byte)cfg.ReadRegister)).MustHaveHappened();
                Assert.Equal(100, fan.CurrentSpeed);
            }

            [Fact]
            public void CallsReadWord()
            {
                var cfg = new FanConfiguration()
                {
                    ReadRegister = 123,
                    MinSpeedValue = 0,
                    MaxSpeedValue = 20000
                };

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);
                A.CallTo(() => ec.ReadWord((byte)cfg.ReadRegister)).Returns((ushort)cfg.MaxSpeedValue);

                var fan = new Fan(ec, cfg, 100, true);
                fan.GetCurrentSpeed();

                A.CallTo(() => ec.ReadWord((byte)cfg.ReadRegister)).MustHaveHappened();
                Assert.Equal(100, fan.CurrentSpeed);
            }
        }

        public class Reset
        {
            [Fact]
            public void CallsWriteByte()
            {
                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var cfg = new FanConfiguration()
                {
                    WriteRegister = 123,
                    ResetRequired = true,
                    FanSpeedResetValue = byte.MaxValue
                };

                var fan = new Fan(ec, cfg, 100, false);
                fan.Reset();

                A.CallTo(() => ec.WriteByte((byte)cfg.WriteRegister, (byte)cfg.FanSpeedResetValue))
                    .MustHaveHappened();
            }

            [Fact]
            public void CallsWriteWord()
            {
                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var cfg = new FanConfiguration()
                {
                    WriteRegister = 123,
                    ResetRequired = true,
                    FanSpeedResetValue = ushort.MaxValue
                };

                var fan = new Fan(ec, cfg, 100, true);
                fan.Reset();

                A.CallTo(() => ec.WriteWord((byte)cfg.WriteRegister, (ushort)cfg.FanSpeedResetValue))
                    .MustHaveHappened();
            }
        }
    }
}
