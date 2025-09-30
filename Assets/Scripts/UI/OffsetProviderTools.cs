using UnityEngine;

namespace Scripts.UI
{
    public class OffsetProviderTools
    {
        public static Vector3 GetTotalOffset(GameObject g)
        {
            Vector3 offset = Vector3.zero;
            foreach (var offsetProvider in g.GetComponents<IOffsetProvider>())
            {
                if (offsetProvider is MonoBehaviour mb && mb.isActiveAndEnabled)
                {
                    offset += offsetProvider.CurrentOffset;
                }
            }

            return offset;
        }
    }   
}