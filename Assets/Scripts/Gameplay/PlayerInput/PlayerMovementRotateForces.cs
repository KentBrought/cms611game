using UnityEngine;

namespace Scripts.Gameplay.PlayerInput
{
    public class PlayerMovementRotateForces : PlayerMovementBase
    {
        [Tooltip("Max velocity change per second")]
        public float maxThrustAcceleration = 10f;
        [Tooltip("Max rotation speed change per second, in degrees.  e.g. 180 means that after increasing rotation for one whole second, the ship will be rotating once every 2 seconds.  Note that angular drag in the rigidbody2d will slow the rotation beyond that.")]
        public float maxAngularAcceleration = 1f;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            var movementDelta = GetMoveInput();
        
            // Grab the up vector of the player's transform.
            var forward = transform.up;
            
            // Apply the thrust force in the direction of the up vector.
            this.cachedRigidbody2D.AddForce(forward * movementDelta.y * this.maxThrustAcceleration * this.cachedRigidbody2D.mass, ForceMode2D.Force);
            
            // Apply the torque force.
            var angularAccelerationInRadians = this.maxAngularAcceleration * Mathf.Deg2Rad;
            this.cachedRigidbody2D.AddTorque(- movementDelta.x * angularAccelerationInRadians * this.cachedRigidbody2D.inertia, ForceMode2D.Force);
        }
    }
}