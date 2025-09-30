using Scripts.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.Gameplay.AI
{
    public class Charge : MonoBehaviour
    {
        public LayerMask targetLayerMask;
        public float chargeSpeed = 10f;
        public float sChargeCooldown = 1f;
        public UnityEvent<GameObject> onChargeStart;
        
        
        protected Rigidbody2D cachedRigidbody2D;
        
        private float _sLastChargeTime = float.MinValue;

        private void Start()
        {
            this.cachedRigidbody2D = GetComponent<Rigidbody2D>();
            if (!this.cachedRigidbody2D)
            {
                Debug.LogWarning($"No Rigidbody2D found on {gameObject.name}.  Needed for {nameof(Charge)}");
            }
        }

        public void ChargeAt(GameObject target)
        {
            if (Time.time - this._sLastChargeTime < this.sChargeCooldown) return;
            
            Vector2 direction = target.transform.position - transform.position;
            direction.Normalize();
            
            // Use Impulse because this is a single instantaneous velocity change.  The parameter is expressed as a speed.
            this.cachedRigidbody2D.AddForce(direction * this.chargeSpeed * this.cachedRigidbody2D.mass, ForceMode2D.Impulse);
            this._sLastChargeTime = Time.time;
            
            this.onChargeStart.Invoke(target);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            MaybeCharge(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            MaybeCharge(other);
        }

        
        private void MaybeCharge(Collider2D other)
        {
            if (GeneralHelpers.IsInMask(this.targetLayerMask, other.gameObject))
            {
                this.ChargeAt(other.gameObject);
            }
        }
    }
}