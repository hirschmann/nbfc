using System;
using Xunit;

namespace StagWare.FanControl.Tests
{
    public class ArithmeticMeanTemperatureFilterTests
    {
        public class Constructor
        {
            [Fact]
            public static void ThrowsWithInvalidPollInterval()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new ArithmeticMeanTemperatureFilter(0));
            }

            [Fact]
            public static void ThrowsWithInvalidTimespan()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new ArithmeticMeanTemperatureFilter(100, 0));
            }
        }

        public class FilterTemperature
        {
            [Fact]
            public static void RespectsTimespan()
            {
                int interval = 2000;
                int timespan = 6000;

                var filter = new ArithmeticMeanTemperatureFilter(interval, timespan);
                double sum = 0;

                for (int i = 0; i < (timespan / interval); i++)
                {
                    double temperature = (i * 5.125);
                    sum += temperature;

                    Assert.Equal(sum / (i + 1), filter.FilterTemperature(temperature));
                }

                sum -= (0 * 5.125);
                sum += 40;

                Assert.Equal(sum / (timespan / interval), filter.FilterTemperature(40));

                sum -= (1 * 5.125);
                sum += -5;

                Assert.Equal(sum / (timespan / interval), filter.FilterTemperature(-5));
            }
        }
    }
}
