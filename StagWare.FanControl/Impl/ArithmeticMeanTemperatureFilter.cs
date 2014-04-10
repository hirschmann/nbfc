using System;
using System.Collections.Generic;

namespace StagWare.FanControl
{
    public class ArithmeticMeanTemperatureFilter : ITemperatureFilter
    {
        #region Constants

        private const int AverageTemperatureTimespan = 6000; //ms

        #endregion

        #region Private Fields

        private readonly int maxSize;
        private Queue<double> queue;
        private double sum;

        #endregion

        #region Constructors

        public ArithmeticMeanTemperatureFilter(int pollInterval)
            : this(pollInterval, AverageTemperatureTimespan)
        {
        }

        public ArithmeticMeanTemperatureFilter(int pollInterval, int timespan)
        {
            if (pollInterval <= 0)
            {
                throw new ArgumentOutOfRangeException("pollInterval", "pollInterval must be greater than 0");
            }

            if (timespan > pollInterval)
            {
                throw new ArgumentOutOfRangeException("timespan", "timespan must be greater or equal to pollInterval");
            }

            this.maxSize = (int)Math.Ceiling((double)timespan / pollInterval);
            this.queue = new Queue<double>(maxSize);
        }

        #endregion

        #region ITemperatureFilter implementation

        public double FilterTemperature(double temperature)
        {
            if (this.queue.Count >= this.maxSize)
            {
                this.sum -= this.queue.Dequeue();
            }

            this.queue.Enqueue(temperature);
            this.sum += temperature;

            return this.sum / this.queue.Count;
        }

        #endregion
    }
}
