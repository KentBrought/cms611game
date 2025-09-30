using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class DistanceDetector : MonoBehaviour
    {
        public float currentAwareness = 0;
        public RangeF clampAwareness = RangeF.Range01;
        public float detectRadius = 10f;
        [Tooltip("Increase currentAwareness when player is in range")]
        public float awarenessPerSecondInRadius = 1; 
        [Tooltip("Decrease currentAwareness when player is out of range")]
        public float awarenessPerSecondOutOfRadius = -0.5f;
        public GameObject detectTarget;
        public LayerMask detectableTargetLayers;
        public ThresholdSignaller noticeThreshold = new ThresholdSignaller();
        public ThresholdSignaller forgetThreshold = new ThresholdSignaller();
        
        protected void FixedUpdate()
        {
            if (!this.detectTarget || !this.detectTarget.activeInHierarchy)
            {
                this.detectTarget = FindNewTarget();
            }
            
            bool canSee = this.detectTarget && 
                          Vector2.Distance(this.transform.position, this.detectTarget.transform.position) <= this.detectRadius;
            
            UpdateAwareness(canSee, Time.fixedDeltaTime);
        }

        GameObject FindNewTarget()
        {
            var nearest = GeneralHelpers.FindClosestTarget(this.transform.position, this.detectRadius, this.detectableTargetLayers);
            return nearest ? nearest.gameObject : null;
        }
        
        protected void UpdateAwareness(bool canSee, float deltaTime)
        {
            // Calculate awareness change
            float deltaPerSecond = canSee ? awarenessPerSecondInRadius : awarenessPerSecondOutOfRadius;
            float delta = deltaTime * deltaPerSecond;
            float newAwareness = this.clampAwareness.Clamp(this.currentAwareness + delta);
            
            
            // Signal for thresholds
            this.noticeThreshold?.SignalForChange(this.currentAwareness, newAwareness);
            this.forgetThreshold?.SignalForChange(this.currentAwareness, newAwareness);
            
            
            // Update awareness with new value
            this.currentAwareness = newAwareness;
        }
    }
}