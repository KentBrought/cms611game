using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Scripts.Helpers
{
    public static class GeneralHelpers
    {
        public static bool IsInMask(LayerMask mask, int layerIndex)
        {
            return (mask.value & (1 << layerIndex)) != 0;
        }
        
        public static bool IsInMask(LayerMask mask, GameObject gameObject)
        {
            return IsInMask(mask, gameObject.layer);
        }


        public static bool SetTextInChildren(Transform textHolder, string showMeText)
        {
            return SetTextInChildren(textHolder.gameObject, showMeText);
        }
        
        public static bool SetTextInChildren(GameObject textHolder, string showMeText)
        {
            TMP_Text tmpText = textHolder.GetComponentInChildren<TMP_Text>();
            if (tmpText == null)
            {
                Debug.LogWarning("No TMP_Text component found on " + textHolder.name);
                return false;
            }

            tmpText.text = showMeText;
            return true;
        }

        public static Vector3 GetMouseWorldPosition(Camera camera)
        {
            var mousePosition = Mouse.current.position.ReadValue();
            var screenPoint = new Vector3(mousePosition.x, mousePosition.y, camera.nearClipPlane);
            var worldPoint = camera.ScreenToWorldPoint(screenPoint);
            return worldPoint;
        }

        public static Camera mainCameraCache = null;
        public static Vector3 GetMouseWorldPosition()
        {
            if (!mainCameraCache)
            {
                mainCameraCache = Camera.main;
            }

            return GetMouseWorldPosition(mainCameraCache);
        }

        public static Vector3 GetRandomPointInside(CircleCollider2D circleCollider2D)
        {
            var normalizedPoint = Random.insideUnitCircle;
            Vector3 offset = normalizedPoint * circleCollider2D.radius;
            return circleCollider2D.transform.position + offset;
        }
        
        public static Vector3 GetRandomPointInside(BoxCollider2D boxCollider2D)
        {
            var size = boxCollider2D.size;
            float halfWidth = size.x / 2;
            float halfHeight = size.y / 2;
            
            var localRandomPoint =
                new Vector2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight)) +
                boxCollider2D.offset;
            
            var worldRandomPoint = boxCollider2D.transform.TransformPoint(localRandomPoint);
            return worldRandomPoint;
        }
        
        public static Vector3 GetRandomPointInside(Collider2D spawnArea)
        {
            switch (spawnArea)
            {
                case BoxCollider2D boxCollider2D:
                    return GetRandomPointInside(boxCollider2D);
                case CircleCollider2D circleCollider2D:
                    return GetRandomPointInside(circleCollider2D);
            }
            
            // No match.  Warn and use the bounds.
            Debug.LogWarning($"GetRandomPointInside: No implementation for {spawnArea.GetType()}");
            var bounds = spawnArea.bounds;
            var randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z));
            return randomPoint;
            
        }

        public static Rect GetLocalBounds(this BoxCollider2D box) // Extension method, looks like a member function of an existing class. Must be a static method in a static class.
        {
            var offset = box.offset + new Vector2(0.5f, 0.5f); // a zero offset means that the collider is centered on the position of the object.
            var size = box.size;
            return new Rect(-offset.x, -offset.y, size.x, size.y);
        }
        
        public static bool Contains(this BoxCollider2D box, Vector3 point)
        {
            var localPoint = box.transform.InverseTransformPoint(point);
            var localBounds = box.GetLocalBounds();
            return localBounds.Contains(localPoint);
        }
        
        public static T ArgMin<T>(this IEnumerable<T> enumerable, Func<T, float> valueSelector)
        {
            var min = float.MaxValue;
            T minElement = default;
            foreach (var element in enumerable)
            {
                var value = valueSelector(element);
                if (value < min)
                {
                    min = value;
                    minElement = element;
                }
            }
            return minElement;
        }
        
        public static void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public static Collider2D FindClosestTarget(Vector2 center, float maxSeekDistance, LayerMask seekLayers)
        {
            var targets = Physics2D.OverlapCircleAll(center, maxSeekDistance, seekLayers);
            return targets.ArgMin(targetCollider => Vector2.Distance(center, targetCollider.transform.position));
        }

        public static Vector3 ToVector3WithZ(this Vector2 v2, float z)
        {
            return new Vector3(v2.x, v2.y, z);
        }
    }
}