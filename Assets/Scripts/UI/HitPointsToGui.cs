using Scripts.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class HitPointsToGui : MonoBehaviour
    {
        public Slider slider;
        public TMP_Text text;
        [Tooltip("Whose hit points to display?  If null, will try to figure it out.")]
        public HitPoints hitPoints;
        
        private void Update()
        {
            if (!this.hitPoints)
            {
                // Grab the first object with hit points that we find active in the scene.
                //  If a non-player has hit points, we'll use that- which is probably a bug.
                //  If there is more than one player active, who knows which one we'll grab?
                //  To be perfectly safe, set the hitPoints field to the correct player's hit points component in the Inspector.
                //  Maybe filter by tag or layer?
                this.hitPoints = FindObjectOfType<HitPoints>();
            }

            if (!this.hitPoints) return;

            if (slider)
            {
                this.slider.value = hitPoints.currentHitPoints;
                this.slider.maxValue = hitPoints.maxHitPoints;
                this.slider.minValue = 0;
            }
            
            if (text)
            {
                this.text.text = hitPoints.currentHitPoints + "/" + hitPoints.maxHitPoints;
            }
        }
    }
}