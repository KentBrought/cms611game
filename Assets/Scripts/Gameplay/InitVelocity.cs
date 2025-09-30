using System.Collections;
using UnityEngine;

namespace Scripts.Gameplay
{
    public class InitVelocity : MonoBehaviour
    {
        [Tooltip("A delay allows players to see the object before it starts moving.  And, perhaps more importantly, it skips any slow rendering frames caused by initializing the scene....")]
        public float sTimeDelay = 0.04f;
        public Vector2 initialVelocity;
        protected bool isInitialized = false;


        void Start()
        {
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            if (this.isInitialized) yield break;

            yield return new WaitForSeconds(this.sTimeDelay);
            
            this.isInitialized = true;
            var rigidBody = GetComponent<Rigidbody2D>();
            if (rigidBody != null)
            {
                rigidBody.velocity = this.initialVelocity;
            }
        }
    }
}