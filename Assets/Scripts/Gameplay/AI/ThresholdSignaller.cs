using System;
using UnityEngine.Events;

namespace Scripts.Gameplay.AI
{
    [Serializable]  // This makes it possible to edit the properties of the class in the inspector.
    public class ThresholdSignaller
    {
        public void SignalForChange(float oldValue, float newValue)
        {
            if (oldValue < newValue && newValue >= this.threshold)
            {
                this.onOverThreshold.Invoke(newValue);
            }
            
            if (oldValue > newValue && newValue <= this.threshold)
            {
                this.onUnderThreshold.Invoke(newValue);
            }
            
            if (oldValue != newValue)
            {
                this.onChange.Invoke(newValue);
            }
        }

        public float threshold = 0;
        public UnityEvent<float> onOverThreshold = new UnityEvent<float>();
        public UnityEvent<float> onUnderThreshold = new UnityEvent<float>();
        public UnityEvent<float> onChange = new UnityEvent<float>();

    }
}