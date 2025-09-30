using Scripts.Helpers;
using Scripts.UI;
using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class GoToLerp : GoToBase
    {
        public AnimationCurve lerpCurve = AnimationCurve.Linear(0,0,1,1);
        public RangeF rMovementDuration = RangeF.Range11;
        
        protected float sMoveStartTime = -1;
        protected float sMoveEndTime = -1;
        protected Vector2 startPosition;
        protected Vector2 endPosition;
        protected bool isMoving = false;

        public override void OnDestinationChanged(Vector2? newDestination)
        {
            base.OnDestinationChanged(newDestination);
            if (!newDestination.HasValue)
            {
                this.isMoving = false;
                return;
            }
            
            this.sMoveStartTime = Time.time;
            this.sMoveEndTime = this.sMoveStartTime + this.rMovementDuration.RandomValue();
            this.isMoving = true;

            this.startPosition = this.transform.position;
            this.endPosition = newDestination.Value;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (Time.fixedTime < this.sMoveStartTime) return;
            if (!this.isMoving) return;
            
            if (Time.fixedTime > this.sMoveEndTime)
            {
                this.transform.position = this.endPosition.ToVector3WithZ(this.transform.position.z);
                this.isMoving = false;
                return;
            }
            
            var fractionOfMove = (Time.fixedTime - this.sMoveStartTime) / (this.sMoveEndTime - this.sMoveStartTime);
            var lerpFractionOfMove = this.lerpCurve.Evaluate(fractionOfMove);
            
            var newPos2d = Vector2.Lerp(this.startPosition, this.endPosition, lerpFractionOfMove);
            this.transform.position = newPos2d.ToVector3WithZ(transform.position.z) +
                                      OffsetProviderTools.GetTotalOffset(this.gameObject);
        }
    }
}