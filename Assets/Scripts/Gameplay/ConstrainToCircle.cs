using UnityEngine;

namespace Scripts.Gameplay
{
    /// <summary>
    /// Note that for this to work reliably, we need to call the FixedUpdate method AFTER other scripts set the position of the object.
    /// One way to do this is to go to Project Settings > Script Execution Order and make ConstrainToCircle execute later than the other scripts.
    /// </summary>
    public class ConstrainToCircle : MonoBehaviour
    {
        public float radius = 1;
        public GameObject centerObject;
        public Vector2 center;
      
        public void FixedUpdate()
        {
            if (this.centerObject) this.center = this.centerObject.transform.position;
            if (this.radius < 0) this.radius = 0;

            var myTransform = this.transform;

            Vector2 myPosition = myTransform.position;
            Vector2 direction = myPosition - this.center;
            float dist = direction.magnitude;
            
            if (!(dist > this.radius)) return;
            
            direction = direction.normalized;
            
            Vector3 newPosition = this.center + direction * this.radius;
            newPosition.z = myTransform.position.z;  // don't move in z direction, we want to keep rendering sort order
            
            myTransform.position = newPosition;
        }
        
        public void SetRadius(float newRadius)
        {
            this.radius = newRadius;
        }
    }
}