using Scripts.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.Gameplay.AI
{
    public abstract class DestinationProvider : MonoBehaviour
    {
        [Tooltip("Will apply whichever Max Seconds is lower.  This one applies even if we never reach a destination.")]
        public RangeF rMaxSecondsBetweenDestinations = RangeF.MaxValue;
        [Tooltip("Will apply whichever Max Seconds is lower.  This one ONLY applies after we reach a destination.")]
        public RangeF rMaxSecondsAtDestination = RangeF.MaxValue;

        
        public Vector2? CurrentDestination
        {
            get => this._destination;
            set
            {
                if (this._destination == value) return;
                
                this.onDestinationChanged.Invoke(value);
                this._destination = value;
            }
        }
        private Vector2? _unreflectedDestination;

        [Tooltip("If defined, forces all generated destinations to be within this bounds")]
        public BoxCollider2D destinationConstraint;
        
        protected float lastAttemptToFindDestinationConstraint = -1000;

        public string constraintTag = "UFO Area";

        public bool IsCloseEnoughToDestination => CurrentDestination.HasValue && Vector2.Distance(this.transform.position, this.CurrentDestination.Value) < this.closeEnoughDistance;
        public float closeEnoughDistance = 0.1f;
        public bool WasCloseEnough { get; protected set; }= false;

        public void ChangeDestinationNow()
        {
            WasCloseEnough = false;
            var newDestination = this.ChooseNewDestinationInWorldSpace();
            SendDestinationChanged(newDestination);
            this.CurrentDestination = newDestination;
            this._unreflectedDestination = newDestination;
        }

        public abstract Vector2? ChooseNewDestinationInWorldSpace();
        
        public UnityEvent<Vector2?> onDestinationChanged;
        public UnityEvent<Vector2> onDestinationReached;
        private Vector2? _destination;

        protected virtual void Start()
        {
            ChangeDestinationNow();
        }

        protected virtual void FixedUpdate()
        {
            ApplyDestinationConstraint();
            
            if (!this.CurrentDestination.HasValue) return;
            
            if (Vector2.Distance(this.transform.position, this.CurrentDestination.Value) < this.closeEnoughDistance)
            {
                this.onDestinationReached.Invoke(this.CurrentDestination.Value);
                WasCloseEnough = true;
            }
        }

        private void ApplyDestinationConstraint()
        {
            float sSinceLastDestinationConstraintUpdate = Time.fixedTime - this.lastAttemptToFindDestinationConstraint;
            
            if (sSinceLastDestinationConstraintUpdate > 1 && 
                (!this.destinationConstraint || !this.destinationConstraint.isActiveAndEnabled))
            {
                var area = string.IsNullOrWhiteSpace(this.constraintTag) ? null : GameObject.FindGameObjectWithTag(constraintTag);
                if (area)
                {
                    this.destinationConstraint = area.GetComponent<BoxCollider2D>();
                }

                this.lastAttemptToFindDestinationConstraint = Time.fixedTime;
            }
            
            if (!this.destinationConstraint) return;
            if (!this.CurrentDestination.HasValue) return;
            
            Rect localBounds = this.destinationConstraint.GetLocalBounds();
            
            // Transform the destination to local space
            Vector2 localDestination = this.destinationConstraint.transform.InverseTransformPoint(this.CurrentDestination.Value);

            // Is local destination inside the collider?
            //  The loop counter protects us from an infinite loop.
            for (int i = 0; i < 100 && !localBounds.Contains(localDestination); i++)
            {
                // Otherwise, ping-pong the destination as if bouncing a ray until it is within the collider.
                localDestination = PingPongIntoRect(localDestination, localBounds);
            }
            
            // Transform the destination back to world space
            this.CurrentDestination = this.destinationConstraint.transform.TransformPoint(localDestination);
        }

        private static float PingPongIntoRange(float value, float min, float max)
        {
            var tooSmallAmount = value - min;
            if (tooSmallAmount < 0)
            {
                value = min + Mathf.Abs(tooSmallAmount);
            }
            
            var tooBigAmount = max - value;
            if (tooBigAmount < 0)
            {
                value = max - Mathf.Abs(tooBigAmount);
            }

            return value;
        }
        
        private static Vector2 PingPongIntoRect(Vector2 localDestination, Rect localBounds)
        {
            localDestination.x = PingPongIntoRange(localDestination.x, localBounds.xMin, localBounds.xMax);
            localDestination.y = PingPongIntoRange(localDestination.y, localBounds.yMin, localBounds.yMax);
            return localDestination;
        }

        protected virtual void SendDestinationChanged(Vector2? newDestination)
        {
            this._unreflectedDestination = newDestination;
            this.onDestinationChanged.Invoke(newDestination);
        }
        
        protected virtual void SendDestinationReached(Vector2 destination)
        {
            this.onDestinationReached.Invoke(destination);
        }
 
        public Collider2D FindClosestTarget(float maxSeekDistance, LayerMask seekLayers)
        {
            return GeneralHelpers.FindClosestTarget(this.transform.position, maxSeekDistance, seekLayers);
        }
        
        
        protected virtual void OnDrawGizmos()
        {
            if (!this.enabled) return;
            if (!this.CurrentDestination.HasValue) return;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(CurrentDestination.Value, this.closeEnoughDistance);
            Gizmos.DrawLine(this.transform.position, CurrentDestination.Value);

            if (this._unreflectedDestination.HasValue &&
                this.destinationConstraint &&
                !this.destinationConstraint.Contains(this._unreflectedDestination.Value))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(this._unreflectedDestination.Value, this.closeEnoughDistance);
                Gizmos.DrawLine(this.transform.position, this._unreflectedDestination.Value);
            }
        }
    }
}