using UnityEngine;

namespace Scripts.Gameplay
{
    public class RotateToFaceVelocity : MonoBehaviour
    {
        public float degFacingOffset = 0;
        public float maxDegreesPerSecond = 180;
        
        protected Rigidbody2D cachedRigidbody;

        protected void OnEnable()
        {
            if (!this.cachedRigidbody)
            {
                this.cachedRigidbody = GetComponent<Rigidbody2D>();
            }
        }

        protected void FixedUpdate()
        {
            if (!this.cachedRigidbody) return;
            
            var velocity = this.cachedRigidbody.velocity;
            var direction = velocity.normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var degrees = angle + degFacingOffset;
            var rotation = Quaternion.AngleAxis(degrees, new Vector3(0,0,1));
            transform.rotation = rotation;
        }
    }
}