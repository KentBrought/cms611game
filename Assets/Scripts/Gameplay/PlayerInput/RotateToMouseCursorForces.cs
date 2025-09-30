using UnityEngine;

namespace Scripts.Gameplay.PlayerInput
{
    // TODO: remove or implement this class
    public class RotateToMouseCursorForces : MonoBehaviour
    {
        public float maxTorqueForce = 1f;

        protected void FixedUpdate()
        {
            // Grab the up vector of the player's transform.
            var forward = transform.up;
            
            // Apply the thrust force in the direction of the up vector.
//            this.cachedRigidbody2D.AddForce(forward * movementDelta.y * this.maxThrustForce, ForceMode2D.Force);
            
            // Apply the torque force.
  //          this.cachedRigidbody2D.AddTorque(- movementDelta.x * this.maxTorqueForce);
        }
    }
}