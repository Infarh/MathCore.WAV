using System;
using System.Collections.Generic;
using System.Text;

namespace MathCore.WAV
{
    public readonly struct Sample
    {
        public double Time { get; }
        public long Value { get; }

        public Sample(double Time, long Value)
        {
            this.Time = Time;
            this.Value = Value;
        }
    }
}
