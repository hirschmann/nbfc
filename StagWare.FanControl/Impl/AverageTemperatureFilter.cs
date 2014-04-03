using System;
using System.Collections.Generic;

namespace StagWare.FanControl
{
    public class AverageTemperatureFilter : ITemperatureFilter
    {
        #region Private Fields

        private readonly int maxSize;
        private Queue<double> queue;
        private double sum;

        #endregion

        #region Constructor

        public AverageTemperatureFilter(int maxQueueSize)
        {
            if (maxQueueSize <= 0)
            {
                throw new ArgumentException("Queue size must be greater than 0");
            }

           this.maxSize = maxQueueSize;
           this.queue = new Queue<double>(maxQueueSize);
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
