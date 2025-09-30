using Scripts.Helpers;
using UnityEngine;

namespace Scripts.UI
{
    public class PopUpTextDuringMouseOver : PopUpText
    {
        protected Collider2D currentCollider;

        protected void Start()
        {
            this.currentCollider = this.GetComponentInChildren<Collider2D>();
        }
        
        protected void FixedUpdate()
        {
            if (this.currentCollider)
            {
                if (this.currentCollider.OverlapPoint(GeneralHelpers.GetMouseWorldPosition()))
                {
                    HandleEnter();
                }
                else
                {
                    HandleExit();
                }
            }
        }

        protected void HandleEnter()
        {
            if (this.currentPopUp) return;

            this.ShowText();
        }
        
        public void HandleExit()
        {
            if (this.currentPopUp == null) return;
            
            Destroy(this.currentPopUp.gameObject);
            this.currentPopUp = null;
        }
    }
}