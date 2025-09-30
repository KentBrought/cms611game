using UnityEngine;

namespace Scripts.Helpers
{
    public class Notes : MonoBehaviour
    {
        [TextArea(3,30)]
        public string notesText;
    }
}