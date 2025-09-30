using System.Collections;
using Scripts.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Scripts.Gameplay.PlayerInput
{
    public class Kicker : InputHandlerBase
    {
        public LayerMask kickableLayers;
        public float kickVelocity = 10f;
        public float sKickCooldown = 1f;
        public float sCoyoteTime = 0.0f;
        public float maxKickRange = 1f;
        public bool addKickerVelocity = true;
        public bool addTargetVelocity = true;
        public string kickMessage = "BAM!";

        public UnityEvent<Kicker, GameObject> onKickSuccessful;
        public UnityEvent<Kicker> onKickAttempted;

        protected float sLastKickTime = float.MinValue;

        protected override void OnEnable()
        {
            base.OnEnable();
            this.Kick.performed += OnKick;
            this.Kick.Enable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.Kick.performed -= OnKick;
            this.Kick.Disable();
        }

        private void OnKick(InputAction.CallbackContext obj)
        {
            // Check cooldown
            if (Time.time - sLastKickTime < sKickCooldown) return;
            
            if (IsGuiAction(obj)) return;
            
            this.onKickAttempted?.Invoke(this);
            
            // Find the closest ball within the max kick range
            var ball = FindClosestBall(maxKickRange, this.kickableLayers);

            if (ball)
            {
                // Hmm, should the cooldown get reset even if there is no ball nearby?
                //  We'd move this outside the if statement to make that happen.
                this.sLastKickTime = Time.time;
                
                if (this.sCoyoteTime > 0)
                {
                    StartCoroutine(KickLater(ball, this.sCoyoteTime));
                }
                else
                {
                    KickNow(ball);
                }
            }
        }

        IEnumerator KickLater(Collider2D ball, float delay)
        {
            yield return new WaitForSeconds(delay);
            KickNow(ball);
        }
        
        private void KickNow(Collider2D ball)
        {
            // Check if the ball is kickable
            var ballRigidBody2d = ball.GetComponent<Rigidbody2D>();
            if (!ballRigidBody2d) return;

            // What direction is the ball from us?
            Vector2 direction = ball.transform.position - transform.position;
            Vector2 newVelocity = direction.normalized * kickVelocity;
            
            if (addKickerVelocity)
            {
                var myRigidBody = GetComponentInParent<Rigidbody2D>();
                newVelocity += myRigidBody.velocity;
            }
            
            if (addTargetVelocity)
            {
                newVelocity += ballRigidBody2d.velocity;
            }

            var deltaVelocity = newVelocity - ballRigidBody2d.velocity;
            
            // Use impulse because it's an instantaneous velocity change.
            ballRigidBody2d.AddForce(deltaVelocity * ballRigidBody2d.mass, ForceMode2D.Impulse);

            if (!string.IsNullOrWhiteSpace(this.kickMessage))
            {
                var popup = GetComponent<PopUpText>();
                if (popup)
                {
                    popup.ShowText(this.kickMessage, ballRigidBody2d.transform);
                }
            }
            
            this.onKickSuccessful?.Invoke(this, ball.gameObject);
        }

        private Collider2D FindClosestBall(float maxDistance, LayerMask kickableLayerMask)
        {
            var ball = Physics2D.OverlapCircleAll(transform.position, maxDistance, kickableLayerMask);
            if (ball.Length > 0)
            {
                var closest = ball[0];
                var closestDistance = Vector2.Distance(transform.position, closest.transform.position);
                for (var i = 1; i < ball.Length; i++)
                {
                    var distance = Vector2.Distance(transform.position, ball[i].transform.position);
                    if (distance < closestDistance)
                    {
                        closest = ball[i];
                        closestDistance = distance;
                    }
                }
                return closest;
            }

            return null;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw a circle around the kicker to show the kick distance in Scene view.
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxKickRange);
        }
    }
  
}