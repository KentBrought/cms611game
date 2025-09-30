using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Helpers
{
    [Serializable]
    public struct RangeF
    {
        public float min;
        public float max;

        public float Width => this.max - min;

        public RangeF(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
        
        public static RangeF Range00 => new RangeF(0, 0);
        public static RangeF Range01 => new RangeF(0, 1);
        public static RangeF Range11 => new RangeF(1, 1);
        public static RangeF MaxValue => new RangeF(float.MaxValue, float.MaxValue);

        public float RandomValue()
        {
            return Random.Range(this.min, this.max);
        }
        
        public float RandomValueBiased(AnimationCurve curve)
        {
            var width = this.Width;
            if (width == 0)
            {
                return this.min;
            }
            
            var biased01 = curve.Evaluate(Random.Range(0f, 1f));
            return min + width * biased01;
        }

        public bool Contains(float time)
        {
            return time >= this.min && time <= this.max; 
        }

        public override string ToString()
        {
            return $"RangeF[{this.min}, {this.max}]";
        }

        public float Lerp(float t)
        {
            return Mathf.Lerp(this.min, this.max, t);
        }

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, this.min, this.max);
        }

        public float Normalize(float input)
        {
            if (input < this.min) return 0;
            if (input > this.max) return 1;
            if (this.Width <= 0) return 0;
            return (input - this.min) / this.Width;
        }
    }
}