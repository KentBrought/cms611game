using Scripts.Helpers;
using UnityEngine;

namespace Scripts.UI
{
    public class ColorSprite : MonoBehaviour
    {
        public SpriteRenderer colorMe;
        
        public RangeF inputRange = RangeF.Range01;
        public Gradient gradient;
        
        
        public void OnNewInput(float input)
        {
            if (!colorMe) return;

            var color = gradient.Evaluate(inputRange.Normalize(input));
            colorMe.color = color;
        }
    }
}