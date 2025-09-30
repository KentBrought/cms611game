using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class DestinationGameObject : DestinationProvider
    {
        public GameObject targetObject;
        public float targetPositionDeltaForDestinationUpdate = 0.1f;

        public override Vector2? ChooseNewDestinationInWorldSpace()
        {
            if (this.targetObject) return this.targetObject.transform.position;

            return null;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (this.targetObject == null) return;

            var needNewDestination = !this.CurrentDestination.HasValue || 
                                     Vector2.Distance(this.targetObject.transform.position, this.CurrentDestination.Value) > this.targetPositionDeltaForDestinationUpdate; 
            
            if (needNewDestination)
            {
                this.CurrentDestination = this.targetObject.transform.position;
            }
        }
    }
}