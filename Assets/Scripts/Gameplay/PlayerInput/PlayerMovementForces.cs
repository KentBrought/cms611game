// Provide player movement based on physical forces as directed by the player.

using UnityEngine;

namespace Scripts.Gameplay.PlayerInput
{
    public class PlayerMovementForces : PlayerMovementBase
    {
        [Tooltip("Max velocity change per second.")]
        public float maxAcceleration = 10f;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            var movementDelta = GetMoveInput();
        
            // Applied as force to simulate a constant acceleration that automatically corrects for the fixed time step.
            this.cachedRigidbody2D.AddForce(movementDelta * (this.maxAcceleration * this.cachedRigidbody2D.mass), ForceMode2D.Force);
        }
    }
}
