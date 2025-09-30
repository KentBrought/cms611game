using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class DestinationRandom : DestinationProvider
    {
        public override Vector2? ChooseNewDestinationInWorldSpace()
        {
            return new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
        }
    }
}