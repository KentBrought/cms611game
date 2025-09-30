using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Gameplay.PlayerInput
{
    public class RotateToMouseCursorSlam : MonoBehaviour
    {
        public float offsetAngleDegrees = 0;
        
        
        private Vector2 GetDesiredLookDirection()
        {
            var worldPoint = GeneralHelpers.GetMouseWorldPosition();
            return worldPoint - this.transform.position;
        }

        void FixedUpdate()
        {
            var desiredLookDirection = GetDesiredLookDirection();
            var angle = Mathf.Atan2(desiredLookDirection.y, desiredLookDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle + this.offsetAngleDegrees, Vector3.forward);
        }
    }
}