using UnityEngine;

namespace Scripts.Gameplay
{
    public class DestroyMe : MonoBehaviour
    {
        public float sDelay = 1;
        
        void Start()
        {
            Destroy(gameObject, sDelay);
        }
    }
}