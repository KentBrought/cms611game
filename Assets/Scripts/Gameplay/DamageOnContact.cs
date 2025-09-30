using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Gameplay
{
    /// Only effective if there is a HitPoints component on the same game object.
    public class DamageOnContact : MonoBehaviour
    {
        public LayerMask thingsThatHurtMe;
        [Tooltip("Damage is only applied if my GameObject also has a HitPoints component.")]
        public float damagePerContact = 1;
        public float sCollisionCooldown = 0.001f;

        protected float sLastDamageTime = float.MinValue;
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (this.sLastDamageTime + this.sCollisionCooldown >= Time.time) return;
            
            if (!GeneralHelpers.IsInMask(this.thingsThatHurtMe, other.gameObject)) return;

            var hitPoints = this.gameObject.GetComponent<HitPoints>();
            if (hitPoints)
            {
                hitPoints.TakeDamage(this.damagePerContact);
                this.sLastDamageTime = Time.time;
            }
        }
    }
}