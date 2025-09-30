using System;
using Scripts.UI;
using UnityEngine;

namespace Scripts.Helpers
{
    [Tooltip("This component exposes Unity's built-in time settings to the Inspector.")]
    public class FrameRateManager : MonoBehaviour
    {
        private const int MinTargetRenderFrameRateAllowed = 1;
        private const int MaxTargetRenderFrameRateAllowed = 200;
        private const float MinTargetPhysicsFrameRateAllowed = .1f;
        private const float MaxTargetPhysicsFrameRateAllowed = 200f;
        private const float MinTimeScale = 0;
        private const float MaxTimeScale = 3;
        private const float MinOfMaxPhysicsTimePerRenderFrame = .01f;
        private const float MaxOfMaxPhysicsTimePerRenderFrame = 1f;
        
        [Header("Normal Time Settings")]
        [Range(MinTargetRenderFrameRateAllowed, MaxTargetRenderFrameRateAllowed)]
        public float targetRenderFrameRate = 60;
        [Range(MinTargetPhysicsFrameRateAllowed, MaxTargetPhysicsFrameRateAllowed)]
        public float targetPhysicsFrameRate = 50;
        [Range(MinOfMaxPhysicsTimePerRenderFrame, MaxOfMaxPhysicsTimePerRenderFrame)]
        public float sMaxPhysicsTimePerRenderFrame = .3333333333333f;        
        
        [Range(MinTimeScale, MaxTimeScale)]
        public float timeScale = 1;

        [Header("Chaos Time Settings")] 
        public bool addFrameRateSpikes = false;
        public SpikeSpec spikeRenderFrameRate = SpikeSpec.Default;

        
        [Serializable]
        public class SpikeSpec
        {
            public RangeF rSecondsBetweenSpikes;
            public RangeF rSpikeDurationInSeconds;
            public RangeF rSpikeValue;

            [Tooltip("Just for debugging.  This tells you what the current spike is.")]
            public SpikeInstance currentSpike;
            
            public static SpikeSpec Default => new SpikeSpec
            {
                // By default, spikes never happen.
                rSecondsBetweenSpikes = new RangeF(float.MaxValue, float.MaxValue),
                rSpikeDurationInSeconds = RangeF.Range00,
                rSpikeValue = RangeF.Range01
            };

            public SpikeInstance RollAndSetSpike()
            {
                this.currentSpike = RollSpike();
                return this.currentSpike;
            }
            
            public SpikeInstance RollSpike()
            {
                var start = this.rSecondsBetweenSpikes.RandomValue() + Time.time;
                var end = start + this.rSpikeDurationInSeconds.RandomValue();
                var newSpike = new SpikeInstance()
                {
                    sSpikeInterval = new RangeF(start, end),
                    spikeValue = this.rSpikeValue.RandomValue()
                };
                
                return newSpike;
            }
        }

        [Serializable]
        public class SpikeInstance
        {
            public RangeF sSpikeInterval;
            public float spikeValue;
            
            public bool IsSpikeActive => this.sSpikeInterval.Contains(Time.time);
        }
        
        
        private void Update()
        {
            if (PauseManager.IsPaused) return;

            this.ApplyFrameRates();
        }

        private void ApplyFrameRates()
        {
            this.targetPhysicsFrameRate = Mathf.Clamp(this.targetPhysicsFrameRate, MinTargetPhysicsFrameRateAllowed,
                MaxTargetPhysicsFrameRateAllowed);
            Time.fixedDeltaTime = 1 / this.targetPhysicsFrameRate;

            this.timeScale = Mathf.Clamp(this.timeScale, MinTimeScale, MaxTimeScale);
            Time.timeScale = this.timeScale;

            this.sMaxPhysicsTimePerRenderFrame = Mathf.Clamp(this.sMaxPhysicsTimePerRenderFrame,
                MinOfMaxPhysicsTimePerRenderFrame, MaxOfMaxPhysicsTimePerRenderFrame);
            Time.maximumDeltaTime = this.sMaxPhysicsTimePerRenderFrame;
            
            ApplyRenderFrameRate();
        }

        /// <summary>
        /// Split out because it's made more complicated by the spike system.
        /// </summary>
        private void ApplyRenderFrameRate()
        {
            this.targetRenderFrameRate = Mathf.Clamp(this.targetRenderFrameRate, MinTargetRenderFrameRateAllowed,
                MaxTargetRenderFrameRateAllowed);

            // Do we need a new spike instance?
            if (null == this.spikeRenderFrameRate.currentSpike || this.spikeRenderFrameRate.currentSpike.sSpikeInterval.max < Time.time)
            {
                // Yes!  Make one.
                this.spikeRenderFrameRate.RollAndSetSpike();
            }
            
            int currentTargetFrameRate = (int)this.targetRenderFrameRate;
            if (this.addFrameRateSpikes && this.spikeRenderFrameRate.currentSpike.IsSpikeActive)
            {
                currentTargetFrameRate = (int)this.spikeRenderFrameRate.currentSpike.spikeValue;
            }

            Application.targetFrameRate = currentTargetFrameRate;
            
        }

        private void OnValidate()
        {
            // Clear any existing spikes, forcing a re-roll.
            this.spikeRenderFrameRate.currentSpike = null;
        }
    }
}