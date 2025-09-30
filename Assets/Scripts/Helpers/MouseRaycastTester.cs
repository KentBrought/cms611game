using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Helpers
{
    public class MouseRaycastTester : MonoBehaviour
    {
        public GameObject currentTarget;
        public GameObject lastTarget;
        public Camera raycastCamera;

        void Start()
        {
            if (!this.raycastCamera)
            {
                this.raycastCamera = Camera.main;
            }
        }
        public void Update()
        {
            Ray ray = this.raycastCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            var hit = Physics2D.GetRayIntersection(ray); 
            if (hit.collider != null)
            {
                this.currentTarget = hit.collider.gameObject;
                this.lastTarget = this.currentTarget;
            }
            else
            {
                this.currentTarget = null;
            }
            
            // if (currentTarget != lastTarget)
            // {
            //     if (lastTarget != null)
            //     {
            //         lastTarget.SendMessage("OnMouseExit");
            //     }
            //     if (currentTarget != null)
            //     {
            //         currentTarget.SendMessage("OnMouseEnter");
            //     }
            // }
        }
    }
}