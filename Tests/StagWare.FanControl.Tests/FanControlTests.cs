using FakeItEasy;
using StagWare.FanControl.Configurations;
using StagWare.FanControl.Plugins;
using System;
using System.Threading.Tasks;
using Xunit;

namespace StagWare.FanControl.Tests
{
    public class FanControlTests
    {
        public class Start
        {
            [Fact]
            public async Task CallsSetTargetSpeed()
            {
                var fanCfg = new FanConfiguration();
                var cfg = new FanControlConfigV2()
                {
                    EcPollInterval = 100,
                    FanConfigurations = { fanCfg }
                };

                var filter = A.Fake<ITemperatureFilter>();
                A.CallTo(() => filter.FilterTemperature(A<double>.Ignored)).ReturnsLazily((double x) => x);

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.IsInitialized).Returns(true);
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var tempMon = A.Fake<ITemperatureMonitor>();
                A.CallTo(() => tempMon.IsInitialized).Returns(true);

                var fan = A.Fake<Fan>(opt => opt.WithArgumentsForConstructor(
                    new object[] { ec, fanCfg, 70, false }));
                A.CallTo(() => fan.GetCurrentSpeed()).Returns(0);

                var tsc = new TaskCompletionSource<bool>();
                Task<bool> task = tsc.Task;

                using (var fanControl = new FanControl(cfg, filter, ec, tempMon, new[] { fan }))
                {
                    fanControl.EcUpdated += (s, e) =>
                    {
                        tsc.TrySetResult(true);
                    };

                    fanControl.Start(false);
                    Assert.True(fanControl.Enabled, nameof(fanControl.Enabled));

                    await Task.WhenAny(task, Task.Delay(cfg.EcPollInterval * 3));

                    Assert.True(task.IsCompleted, nameof(task.IsCompleted));
                    A.CallTo(() => fan.SetTargetSpeed(A<float>.Ignored, A<float>.Ignored, false))
                                        .MustHaveHappened();
                }
            }

            [Fact]
            public async Task RespectsReadOnlyArg()
            {
                var fanCfg = new FanConfiguration();
                var cfg = new FanControlConfigV2()
                {
                    EcPollInterval = 100,
                    FanConfigurations = { fanCfg }
                };

                var filter = A.Fake<ITemperatureFilter>();
                A.CallTo(() => filter.FilterTemperature(A<double>.Ignored)).ReturnsLazily((double x) => x);

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.IsInitialized).Returns(true);
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var tempMon = A.Fake<ITemperatureMonitor>();
                A.CallTo(() => tempMon.IsInitialized).Returns(true);

                var fan = A.Fake<Fan>(opt => opt.WithArgumentsForConstructor(
                    new object[] { ec, fanCfg, 70, false }));
                A.CallTo(() => fan.GetCurrentSpeed()).Returns(0);

                var tsc = new TaskCompletionSource<bool>();
                Task<bool> task = tsc.Task;

                using (var fanControl = new FanControl(cfg, filter, ec, tempMon, new[] { fan }))
                {
                    fanControl.EcUpdated += (s, e) =>
                    {
                        tsc.TrySetResult(true);
                    };

                    fanControl.Start(true);
                    Assert.True(fanControl.Enabled, nameof(fanControl.Enabled));
                    Assert.True(fanControl.ReadOnly, nameof(fanControl.ReadOnly));                    

                    await Task.WhenAny(task, Task.Delay(cfg.EcPollInterval * 3));

                    Assert.True(task.IsCompleted, nameof(task.IsCompleted));
                    A.CallTo(() => fan.SetTargetSpeed(A<float>.Ignored, A<float>.Ignored, false))
                                        .MustNotHaveHappened();
                }
            }

            [Fact]
            public void AppliesRegisterWriteConfigurations()
            {
                var fanCfg = new FanConfiguration();
                var registerWriteCfg = new RegisterWriteConfiguration()
                {
                    Register = 123,
                    Value = 12,
                    WriteOccasion = RegisterWriteOccasion.OnInitialization
                };

                var cfg = new FanControlConfigV2()
                {
                    EcPollInterval = 100,
                    FanConfigurations = { fanCfg },
                    RegisterWriteConfigurations = { registerWriteCfg }
                };

                var filter = A.Fake<ITemperatureFilter>();
                A.CallTo(() => filter.FilterTemperature(A<double>.Ignored)).ReturnsLazily((double x) => x);

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.IsInitialized).Returns(true);
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var tempMon = A.Fake<ITemperatureMonitor>();
                A.CallTo(() => tempMon.IsInitialized).Returns(true);

                using (var fanControl = new FanControl(cfg, filter, ec, tempMon))
                {
                    fanControl.Start(false);

                    A.CallTo(() => ec.WriteByte((byte)registerWriteCfg.Register, (byte)registerWriteCfg.Value))
                        .MustHaveHappened();
                }
            }
        }

        public class SetTargetFanSpeed
        {
            [Theory]
            [InlineData(-1)]
            [InlineData(0)]
            [InlineData(66.66)]
            [InlineData(111)]
            public async Task CallsSetTargetSpeed(float speed)
            {
                var fanCfg1 = new FanConfiguration();
                var fanCfg2 = new FanConfiguration();

                var cfg = new FanControlConfigV2()
                {
                    EcPollInterval = 100,
                    FanConfigurations = { fanCfg1, fanCfg2 }
                };

                var filter = A.Fake<ITemperatureFilter>();
                A.CallTo(() => filter.FilterTemperature(A<double>.Ignored)).ReturnsLazily((double x) => x);

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.IsInitialized).Returns(true);
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var tempMon = A.Fake<ITemperatureMonitor>();
                A.CallTo(() => tempMon.IsInitialized).Returns(true);

                var fan1 = A.Fake<Fan>(opt => opt.WithArgumentsForConstructor(
                    new object[] { ec, fanCfg1, 70, false }));
                A.CallTo(() => fan1.GetCurrentSpeed()).Returns(0);

                var fan2 = A.Fake<Fan>(opt => opt.WithArgumentsForConstructor(
                    new object[] { ec, fanCfg2, 70, false }));
                A.CallTo(() => fan2.GetCurrentSpeed()).Returns(0);

                var tsc = new TaskCompletionSource<bool>();
                Task<bool> task = tsc.Task;

                var fanControl = new FanControl(cfg, filter, ec, tempMon, new[] { fan1, fan2 });
                fanControl.EcUpdated += (s, e) =>
                {
                    tsc.TrySetResult(true);
                };

                fanControl.SetTargetFanSpeed(speed, 0);
                fanControl.SetTargetFanSpeed(speed, 1);
                fanControl.Start(false);

                Assert.True(fanControl.Enabled, nameof(fanControl.Enabled));

                await Task.WhenAny(task, Task.Delay(cfg.EcPollInterval * 3));

                Assert.True(task.IsCompleted, nameof(task.IsCompleted));
                A.CallTo(() => fan1.SetTargetSpeed(speed, A<float>.Ignored, false))
                                    .MustHaveHappened();
                A.CallTo(() => fan2.SetTargetSpeed(speed, A<float>.Ignored, false))
                                    .MustHaveHappened();
            }

            [Fact]
            public void ThrowsWhenIndexIsInvalid()
            {
                var fanCfg = new FanConfiguration();
                var cfg = new FanControlConfigV2()
                {
                    EcPollInterval = 100,
                    FanConfigurations = { fanCfg }
                };

                var filter = A.Fake<ITemperatureFilter>();
                A.CallTo(() => filter.FilterTemperature(A<double>.Ignored)).ReturnsLazily((double x) => x);

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.IsInitialized).Returns(true);
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var tempMon = A.Fake<ITemperatureMonitor>();
                A.CallTo(() => tempMon.IsInitialized).Returns(true);

                using (var fanControl = new FanControl(cfg, filter, ec, tempMon))
                {
                    var exception = Record.Exception(() => fanControl.SetTargetFanSpeed(123, 12));

                    Assert.NotNull(exception);
                    Assert.IsType<IndexOutOfRangeException>(exception);
                }
            }
        }

        public class Stop
        {
            [Fact]
            public void CallsResetOnFans()
            {
                var fanCfg = new FanConfiguration()
                {
                    ResetRequired = true
                };

                var cfg = new FanControlConfigV2()
                {
                    EcPollInterval = 100,
                    FanConfigurations = { fanCfg }
                };

                var filter = A.Fake<ITemperatureFilter>();
                A.CallTo(() => filter.FilterTemperature(A<double>.Ignored)).ReturnsLazily((double x) => x);

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.IsInitialized).Returns(true);
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var tempMon = A.Fake<ITemperatureMonitor>();
                A.CallTo(() => tempMon.IsInitialized).Returns(true);

                var fan = A.Fake<Fan>(opt => opt.WithArgumentsForConstructor(
                    new object[] { ec, fanCfg, 70, false }));
                A.CallTo(() => fan.GetCurrentSpeed()).Returns(0);

                using (var fanControl = new FanControl(cfg, filter, ec, tempMon, new[] { fan }))
                {
                    fanControl.Start(false);
                    fanControl.Stop();

                    A.CallTo(() => fan.Reset()).MustHaveHappened();
                }
            }

            [Fact]
            public void ResetsRegisterWriteConfigurations()
            {
                var fanCfg = new FanConfiguration();
                var regWriteCfg = new RegisterWriteConfiguration()
                {
                    Register = 123,
                    Value = 12,
                    ResetRequired = true,
                    ResetValue = 24
                };

                var cfg = new FanControlConfigV2()
                {
                    EcPollInterval = 100,
                    FanConfigurations = { fanCfg },
                    RegisterWriteConfigurations = { regWriteCfg }
                };

                var filter = A.Fake<ITemperatureFilter>();
                A.CallTo(() => filter.FilterTemperature(A<double>.Ignored)).ReturnsLazily((double x) => x);

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.IsInitialized).Returns(true);
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);
                A.CallTo(() => ec.ReadByte(A<byte>.Ignored)).Returns((byte)0);

                var tempMon = A.Fake<ITemperatureMonitor>();
                A.CallTo(() => tempMon.IsInitialized).Returns(true);

                using (var fanControl = new FanControl(cfg, filter, ec, tempMon))
                {
                    fanControl.Start(false);
                    fanControl.Stop();

                    A.CallTo(() => ec.WriteByte((byte)regWriteCfg.Register, (byte)regWriteCfg.ResetValue))
                        .MustHaveHappened();
                }
            }
        }

        public class Dispose
        {
            [Fact]
            public void CallsResetOnFans()
            {
                var fanCfg = new FanConfiguration()
                {
                    ResetRequired = true
                };

                var cfg = new FanControlConfigV2()
                {
                    EcPollInterval = 100,
                    FanConfigurations = { fanCfg }
                };

                var filter = A.Fake<ITemperatureFilter>();
                A.CallTo(() => filter.FilterTemperature(A<double>.Ignored)).ReturnsLazily((double x) => x);

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.IsInitialized).Returns(true);
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var tempMon = A.Fake<ITemperatureMonitor>();
                A.CallTo(() => tempMon.IsInitialized).Returns(true);

                var fan = A.Fake<Fan>(opt => opt.WithArgumentsForConstructor(
                    new object[] { ec, fanCfg, 70, false }));
                A.CallTo(() => fan.GetCurrentSpeed()).Returns(0);

                using (var fanControl = new FanControl(cfg, filter, ec, tempMon, new[] { fan }))
                {
                    fanControl.Start(false);                    
                }

                A.CallTo(() => fan.Reset()).MustHaveHappened();
            }

            [Fact]
            public void TriesToForceResetFans()
            {
                var fanCfg = new FanConfiguration()
                {
                    ResetRequired = true
                };

                var cfg = new FanControlConfigV2()
                {
                    EcPollInterval = 100,
                    FanConfigurations = { fanCfg }
                };

                var filter = A.Fake<ITemperatureFilter>();
                A.CallTo(() => filter.FilterTemperature(A<double>.Ignored)).ReturnsLazily((double x) => x);

                var ec = A.Fake<IEmbeddedController>();
                A.CallTo(() => ec.IsInitialized).Returns(true);
                A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(true);

                var tempMon = A.Fake<ITemperatureMonitor>();
                A.CallTo(() => tempMon.IsInitialized).Returns(true);

                var fan = A.Fake<Fan>(opt => opt.WithArgumentsForConstructor(
                    new object[] { ec, fanCfg, 70, false }));
                A.CallTo(() => fan.GetCurrentSpeed()).Returns(0);

                using (var fanControl = new FanControl(cfg, filter, ec, tempMon, new[] { fan }))
                {
                    fanControl.Start(false);
                    A.CallTo(() => ec.AcquireLock(A<int>.Ignored)).Returns(false);
                }

                A.CallTo(() => fan.Reset()).MustHaveHappened();
            }
        }
    }
}
