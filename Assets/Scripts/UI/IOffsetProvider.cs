using UnityEngine;

namespace Scripts.UI
{
    public interface IOffsetProvider
    {
        public Vector3 CurrentOffset { get; }
    }
}