using System.Collections;
using Scripts.Gameplay.PlayerInput;
using Scripts.Helpers;
using UnityEngine;

namespace Scripts.UI
{
    public class KickSpangSpawner : MonoBehaviour
    {
        public GameObject kickSpangPrefab;
        [Tooltip("Normalized size of the kick spang over the spangDuration. 1 = max kicker range, 0 = no size.  Time is normalized as well, so the spang expires at t=1 on the curve.")]
        public AnimationCurve sizeCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float spangDuration = 0.5f;

        public void SpawnSpang(Kicker kicker)
        {
            StartCoroutine(SpawnSpangCoroutine(kicker));
        }
        
        
        public IEnumerator SpawnSpangCoroutine(Kicker kicker)
        {
            if (!kickSpangPrefab) yield break;
            
            var kickSpang = Instantiate(kickSpangPrefab, kicker.transform.position, Quaternion.identity);

            var spangRenderer = kickSpang.GetComponent<Renderer>();
            if (!spangRenderer) yield break; //  no visuals, no need to continue

            var bounds = spangRenderer.bounds;
            float baseSize = Mathf.Max(bounds.size.x, bounds.size.y);
            if (baseSize == 0) yield break; // no size, no need to continue
            
            float startTime = Time.time;
            
            // maxKickRange is a radius, size is more like a diameter. 
            RangeF sizeRange = new RangeF(0, 2 * kicker.maxKickRange);
            
            while (Time.time - startTime < spangDuration && kickSpang)
            {
                float t = (Time.time - startTime) / spangDuration;
                kickSpang.transform.localScale = Vector3.one * sizeRange.Lerp(this.sizeCurve.Evaluate(t)) / baseSize;
                yield return null;
            }
            
            Destroy(kickSpang);            
        }
    }
}