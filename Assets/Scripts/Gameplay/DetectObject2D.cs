using System.Collections.Generic;
using Scripts.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.Gameplay
{
    public class DetectObject2D : MonoBehaviour 
    {
        public List<Collider2D> interestingObjects = new List<Collider2D>();
        public LayerMask objectLayerMask;
        public UnityEvent onObjectEnter;

        private void Start()
        {
            var myCollider = GetComponent<Collider2D>();
            if (!myCollider)
            {
                Debug.LogWarning($"No collider found on {this.name} for DetectObject2D");
            }
            else if (!myCollider.isTrigger)
            {
                Debug.LogWarning($"Collider on {this.name} must be a trigger for DetectObject2D to work!");
            }
            
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (GeneralHelpers.IsInMask(this.objectLayerMask, other.gameObject) || this.interestingObjects.Contains(other))
            {
                this.onObjectEnter.Invoke();
            }
        }
    }
}