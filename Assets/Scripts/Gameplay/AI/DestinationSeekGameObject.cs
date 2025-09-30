using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class DestinationSeekGameObject : DestinationGameObject
    {
        public LayerMask targetLayers;
        public float seekDistance = 10f;
        public float forgetDistance = 20f;
        
        protected override void FixedUpdate()
        {
            // Base class updates the destination for us.  Not worried if it's a frame behind.
            base.FixedUpdate();
            
            // Forget target if it is too far away
            if (this.targetObject && Vector2.Distance(this.transform.position, this.targetObject.transform.position) > this.forgetDistance)
            {
                this.targetObject = null;
            }

            if (this.targetObject != null) return;
            
            // Get a new target
            var nearestCollider = FindClosestTarget();
            if (nearestCollider != null)
            {
                this.targetObject = nearestCollider.gameObject;
            }
        }

        private Collider2D FindClosestTarget()
        {
            return FindClosestTarget(this.seekDistance, this.targetLayers);
        }

        public override Vector2? ChooseNewDestinationInWorldSpace()
        {
            this.targetObject = null;
            return null;
        }
    }
}