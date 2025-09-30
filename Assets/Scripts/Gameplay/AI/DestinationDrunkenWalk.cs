using Scripts.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Gameplay.AI
{
    public class DestinationDrunkenWalk : DestinationProvider
    {
        [Tooltip("How far away is the next destination from the current position?")]
        public RangeF rStepSize = new RangeF(0.5f, 1.0f);

        [Header("Debug")] 
        public float sDestinationTimeOut;
        
        public override Vector2? ChooseNewDestinationInWorldSpace()
        {
            sDestinationTimeOut = Time.time + this.rMaxSecondsBetweenDestinations.RandomValue();
            
            var stepSize = rStepSize.RandomValue();
            float angle = Random.Range(0, Mathf.PI * 2);
            
            return this.transform.position + new Vector3(stepSize * Mathf.Cos(angle), stepSize * Mathf.Sin(angle), 0);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (Time.time > sDestinationTimeOut)
            {
                ChangeDestinationNow();
            }
        }

        protected override void SendDestinationReached(Vector2 destination)
        {
            base.SendDestinationReached(destination);
            sDestinationTimeOut = Mathf.Min(Time.time + this.rMaxSecondsAtDestination.RandomValue(), this.sDestinationTimeOut);
        }

    }
}