using Scripts.Helpers;
using UnityEngine;

namespace Scripts.UI
{
    public class PopUpText : MonoBehaviour
    {
        [Tooltip("This prefab is used to display the text and should have a TextMeshPro or TextMeshProUGUI component.")]
        public GameObject showMePrefab;
        public float sShowDuration = 1f;
        public string showMeText;

        public SpawnInfo spawnInfo = new SpawnInfo();
        
        protected Transform currentPopUp;

        
        public void ShowText(string textToShow, Transform relativePositionProvider, Vector3 extraOffset)
        {
            var showMe = this.spawnInfo.Spawn(relativePositionProvider, this.showMePrefab.transform);
            showMe.transform.position += extraOffset;
            
            GeneralHelpers.SetTextInChildren(showMe, textToShow);
            Destroy(showMe.gameObject, sShowDuration);  // wait a while, then destroy the popup text
            this.currentPopUp = showMe.transform;
        }

        
        public void ShowText(string textToShow)
        {
            ShowText(textToShow, this.transform, Vector3.zero);
        }

        public void ShowText()
        {
            this.ShowText(this.showMeText);
        }

        public void ShowText(string textToShow, Vector3 offset)
        {
            ShowText(textToShow, this.transform, offset);
        }   
        
        public void ShowText(string textToShow, Transform relativePositionProvider)
        {
            ShowText(textToShow, relativePositionProvider, Vector3.zero);
        }

        private void OnDisable()
        {
            if (this.currentPopUp != null)
            {
                Destroy(this.currentPopUp.gameObject);
                this.currentPopUp = null;
            }
        }
    }
}
