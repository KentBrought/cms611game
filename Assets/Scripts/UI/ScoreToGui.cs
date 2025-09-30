using Scripts.Gameplay;
using TMPro;
using UnityEngine;

namespace Scripts.UI
{
    public class ScoreToGui : MonoBehaviour
    {
        public TMP_Text text;
        
        private void Update()
        {
            if (text && GameManager.Instance)
            {
                text.text = $"Score: {GameManager.Instance.score}";
            }
        }
    
    }
}