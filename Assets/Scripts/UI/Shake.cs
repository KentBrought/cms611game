using System.Collections;
using Scripts.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.UI
{
    public class Shake : MonoBehaviour, IOffsetProvider
    {
        public Vector3 CurrentOffset => this.currentShakeOffset;
        protected Vector2 currentShakeOffset = Vector2.zero;
        protected float sCurrentFinishTime = -1;
        public float sTotalShakeDuration = 0.0f;
        public RangeF sSingleShakeDuration = RangeF.Range00;
        public Vector2 maxShakeOffset = Vector2.one * 0.05f;
        public AnimationCurve lerpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        protected Coroutine currentShakeCoroutine;

        
        public void Start() //  The existence of this function allows us to enable/disable in the Inspector
        {
        }

        public void StartShake()
        {
            currentShakeCoroutine = StartCoroutine(CoroutineAnimateShakeOffset());
        }

        protected IEnumerator CoroutineAnimateShakeOffset()
        {
            if (this.sCurrentFinishTime > Time.time) yield break; // Already shaking!
            if (!this.isActiveAndEnabled) yield break;
            
            Vector2 lastOffset = Vector2.zero;
            this.sCurrentFinishTime = Time.time + this.sTotalShakeDuration;
            float elapsedTime = 0;
            
            while (elapsedTime < this.sTotalShakeDuration && this.isActiveAndEnabled)
            {
                // Do one shake.
                float sCurrentSingleShakeDuration = this.sSingleShakeDuration.RandomValue();
                float sStartSingleShake = elapsedTime;
                float sEndSingleShake = sStartSingleShake + sCurrentSingleShakeDuration;
                Vector2 nextOffset = new Vector2( this.maxShakeOffset.x * Random.value,
                    this.maxShakeOffset.y * Random.value);
                
                if (sEndSingleShake >= this.sTotalShakeDuration)
                {
                    sEndSingleShake = this.sTotalShakeDuration;
                    nextOffset = Vector2.zero;
                }

                while (elapsedTime < sEndSingleShake && elapsedTime < this.sTotalShakeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    var singleShakeElapsed = elapsedTime - sStartSingleShake;
                    var percentComplete = singleShakeElapsed / sCurrentSingleShakeDuration;

                    this.currentShakeOffset = Vector2.Lerp( lastOffset, nextOffset, this.lerpCurve.Evaluate(percentComplete));
                    // Wait for the next rendered frame.
                    yield return null;
                }
                
                // Set up for next shake.
                lastOffset = nextOffset;
            }

            this.currentShakeOffset = Vector2.zero;
            this.sCurrentFinishTime = -1;
        }

    }
}