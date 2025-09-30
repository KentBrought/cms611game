using UnityEngine;

namespace Scripts.UI
{
    [ExecuteInEditMode]
    public class LetterBoxCamera : MonoBehaviour
    {
        public bool updateInEditor = true;
        public Vector2 targetAspectRatio = new Vector2(16, 9);
        public bool setPositionToo = false;
        protected Camera cachedCamera;
        protected Vector3? initialLocalPosition;
        
        void Start()
        {
            cachedCamera = GetComponent<Camera>();
            if (this.cachedCamera)
            {
                
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!this.updateInEditor && !Application.isPlaying) return;
            if (this.targetAspectRatio.x <= 0 || this.targetAspectRatio.y <= 0) return;
            if (Screen.width <= 0 || Screen.height <= 0) return;
            if (!cachedCamera) return;
            
            // set the desired aspect ratio
            float targetAspect = targetAspectRatio.x / targetAspectRatio.y;
            
            // determine the game window's current aspect ratio
            float windowAspect = Screen.width / (float)Screen.height;
            
            // current viewport height should be scaled by this amount
            float scaleHeight = windowAspect / targetAspect;
            
            // if scaled height is less than the current height, add letterbox
            if (scaleHeight < 1.0f)
            {
                Rect rect = new Rect
                {
                    width = 1.0f,
                    height = scaleHeight,
                    x = 0,
                    y = (1.0f - scaleHeight) / 2.0f
                };

                this.cachedCamera.rect = rect;
            }
            else // add pillar box
            {
                float scaleWidth = 1.0f / scaleHeight;

                Rect rect = new Rect
                {
                    width = scaleWidth,
                    height = 1.0f,
                    x = (1.0f - scaleWidth) / 2.0f,
                    y = 0
                };

                this.cachedCamera.rect = rect;
            }

            if (!Application.isPlaying) return;
            
            // Init initial position if it has not been done yet.
            this.initialLocalPosition ??= this.transform.localPosition;

            if (this.setPositionToo)
            {
                this.transform.localPosition = this.initialLocalPosition.Value + OffsetProviderTools.GetTotalOffset(this.gameObject);
            }
        }
    }
}