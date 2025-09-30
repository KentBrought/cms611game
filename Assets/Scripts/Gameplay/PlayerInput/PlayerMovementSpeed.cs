// Provide player movement based on directly setting its velocity.

using UnityEngine;

namespace Scripts.Gameplay.PlayerInput
{
    public class PlayerMovementSpeed : PlayerMovementBase
    {
        [Tooltip("Max velocity per second.")]
        public float maxSpeed = 10f;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            var movementDelta = GetMoveInput();

            // Instantly set the velocity of the player to match the input.
            this.cachedRigidbody2D.velocity = movementDelta * this.maxSpeed;
        }
    }
}
