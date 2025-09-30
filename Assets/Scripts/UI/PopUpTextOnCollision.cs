using Scripts.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.UI
{
    public class PopUpTextOnCollision : PopUpText
    {
        public LayerMask whenCollingWithLayer;
        public UnityEvent<GameObject> onPopUp;
        protected int lastCollisionPopUpFrame = int.MinValue; 

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (this.lastCollisionPopUpFrame >= Time.frameCount) return;
            if (!GeneralHelpers.IsInMask(this.whenCollingWithLayer, other.gameObject)) return;
            
            this.lastCollisionPopUpFrame = Time.frameCount;
            this.ShowText();
            this.onPopUp.Invoke(other.gameObject);
        }
    }
}
