using System;
using System.Collections;
using Scripts.Gameplay;
using UnityEngine;

namespace Scripts.Helpers
{
    /// <summary>
    /// A handy place to stick helpful calls that we want to wire up to Unity events.
    /// </summary>
    public class HelpersOnMonoBehaviour : MonoBehaviour
    {
        public void QuitApplication()
        {
            // You'd think that Application.Quit() would be enough, but it's not.
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_WEBPLAYER
                Application.OpenURL(webplayerQuitURL);
            #else
                Application.Quit();
            #endif
        }
        
        public void ReloadScene()
        {
            GeneralHelpers.ReloadScene();
        }

        public void ResetDelayed(float seconds)
        {
            StartCoroutine(CallLaterCoroutine(seconds, this.Reset));
        }
        
        public void Reset()
        {
            if (!this) return;
            
            ResetToStart.ResetAll();
        }

        public IEnumerator CallLaterCoroutine(float seconds, Action a)
        {
            yield return new WaitForSeconds(seconds);

            if (!this) yield break;
            
            a.Invoke();
        }

        public struct PlaySpec
        {
            public AudioSource source;
            public  AudioClip clip;
            public  RangeF volumeRange;
        }

        public void PlayOneShot01(AudioClip clip)
        {
            var spec = new PlaySpec()
            {
                source = GetComponentInParent<AudioSource>(),
                clip = clip,
                volumeRange = RangeF.Range01
            };
            
            PlayOneShot(spec);
        }
        
        public void PlayOneShot(PlaySpec spec)
        {
            if (!spec.source) return;
            if (!spec.clip) return;

            spec.source.PlayOneShot(spec.clip, spec.volumeRange.RandomValue());
        }

        public void AddToScore(int deltaScore)
        {
            if (GameManager.Instance)
            {
                GameManager.Instance.score += deltaScore;
            }
        }
    }
}