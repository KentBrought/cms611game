using UnityEngine;
using UnityEngine.Events;

namespace Scripts.Gameplay
{
    public class HitPoints : MonoBehaviour
    {
        public float maxHitPoints = 100;
        public float currentHitPoints = 100;
        public UnityEvent onDamaged;
        public UnityEvent onHealed;
        public UnityEvent onKilled;
        
        public bool IsAlive => currentHitPoints > 0;


        public void TakeDamage(float damage)
        {
            var wasDead = !IsAlive;
            
            if (damage < 0)
            {
                this.onHealed.Invoke();
                this.currentHitPoints -= damage;
                return;
            }

            if (damage > 0 && !wasDead)
            {
                this.currentHitPoints -= damage;

                if (this.IsAlive)
                {
                    this.onDamaged.Invoke();
                }
                else
                {
                    this.onKilled.Invoke();
                }

                return;
            }
            
            // Damage was zero, or we were dead.
            // For grins, record it anyway but don't do events.
            this.currentHitPoints -= damage;
        }
    }
}