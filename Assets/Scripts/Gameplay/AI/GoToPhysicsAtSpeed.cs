using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class GoToPhysicsAtSpeed : GoToBase
    {
        public float maxAcceleration = 10f;
        public RangeF rSpeed = RangeF.Range01;
        public AnimationCurve speedMultiplierAtNormalizedDistance = AnimationCurve.Constant(0,1,1);
        public float normalizedDistanceDenominator = 10;

        protected float speedThisTime;

        public override void OnDestinationChanged(Vector2? newDestination)
        {
            base.OnDestinationChanged(newDestination);

            this.speedThisTime = this.rSpeed.RandomValue();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (!this.cachedRigidbody2D) return;
           if (!this.DestinationProvider) return;
           if (!this.DestinationProvider.CurrentDestination.HasValue) return;
            
            Vector2 toDestination = this.DestinationProvider.CurrentDestination.Value - (Vector2) transform.position;
            var desiredVelocity = CalculateDesiredVelocity(toDestination);

            Vector2 desiredDeltaV = desiredVelocity - this.cachedRigidbody2D.linearVelocity;
            Vector2 desiredAcceleration = desiredDeltaV / Time.fixedDeltaTime;
            
            // Cap acceleration.
            var acceleration = Mathf.Min(desiredAcceleration.magnitude, this.maxAcceleration);
            
            this.cachedRigidbody2D.AddForce(desiredDeltaV.normalized * (acceleration * this.cachedRigidbody2D.mass), ForceMode2D.Force);
        }

        private Vector2 CalculateDesiredVelocity(Vector2 toDestination)
        {
            float toDestinationMagnitude = toDestination.magnitude;                
            if (DestinationProvider.IsCloseEnoughToDestination || 0 == toDestinationMagnitude)
            {
                return Vector2.zero;
            }

            float normalizedDistance = this.normalizedDistanceDenominator != 0 ? toDestinationMagnitude / this.normalizedDistanceDenominator : 1;
            float speedMultiplierForDistance = this.speedMultiplierAtNormalizedDistance.Evaluate(normalizedDistance);
            var desiredSpeed = this.speedThisTime * speedMultiplierForDistance;

            // Will this speed shoot past our target?
            if (desiredSpeed * Time.fixedDeltaTime > toDestinationMagnitude)
            {
                desiredSpeed = toDestinationMagnitude / Time.fixedDeltaTime;
            }
            
            var toDestinationNormalized = toDestination / toDestinationMagnitude;
            var desiredVelocity = toDestinationNormalized * desiredSpeed;
            return desiredVelocity;
        }
    }
}