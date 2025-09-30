using System;
using Scripts.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts.Helpers
{
    // Must be Serializable to be visible in the inspector and saved with the scene/prefab.
    [Serializable]
    public class SpawnInfo
    {
        public bool allowSpawnWhenPaused = false;
        
        [Header("Position")]
        public Vector3 spawnPosition;
        public bool setSpawnPosition = true;
        public bool rotateSpawnPosition = true;
        public bool addSpawnPosition = true;

        [Header("Velocity")]
        public bool setSpawnVelocity = true;
        [Tooltip("True if want to rotate the velocity based on the spawn parent's rotation.")]
        public bool rotateSpawnVelocity = true;
        [Tooltip("True if want to modify the velocity based on the spawn parent's velocity.")]
        public bool addSpawnVelocity = true;
        public RangeF spawnSpeed = RangeF.Range00;
        public RangeF degSpawnVelocityAngle = RangeF.Range00;
        
        [Header("Scale")]
        public Vector3 extraScale = Vector3.one;
        
    
        
        
        public void SetChildSpawnPosition(Transform relativePositionProvider, Transform child)
        {
            if (!this.setSpawnPosition) return;

            Vector3 desiredPosition = CalculateDesiredPosition(relativePositionProvider);

            child.transform.position = desiredPosition;
        }

        private Vector3 CalculateDesiredPosition(Transform relativePositionSource)
        {
            Vector3 desiredPosition = this.spawnPosition;

            if (!relativePositionSource) return desiredPosition;
            
            if (this.rotateSpawnPosition)
            {
                desiredPosition = relativePositionSource.rotation * desiredPosition;
            }

            if (this.addSpawnPosition)
            {
                desiredPosition += relativePositionSource.position;
            }

            return desiredPosition;
        }

        public void SetChildSpawnVelocity(Transform relativeVelocityProvider, Transform child)
        {
            if (!this.setSpawnVelocity) return;
            
            var spawnedRigidBody = child.GetComponent<Rigidbody2D>();
            if (!spawnedRigidBody)
            {
                Debug.LogWarning($"Cannot set velocity of spawned object {child.name} because it has no Rigidbody2D component.");
                return;
            }

            var degVelocityAngle = this.degSpawnVelocityAngle.RandomValue();
            var radVelocityAngle = degVelocityAngle * Mathf.Deg2Rad;
            var velocity = new Vector2(Mathf.Cos(radVelocityAngle), Mathf.Sin(radVelocityAngle) * this.spawnSpeed.RandomValue());
            
            if (this.rotateSpawnVelocity)
            {
                velocity = relativeVelocityProvider.rotation * velocity;
            }
            
            if (this.addSpawnVelocity)
            {
                var parentRigidBody = relativeVelocityProvider.GetComponent<Rigidbody2D>();
                if (parentRigidBody)
                {
                    velocity += parentRigidBody.velocity;
                }
                else
                {
                    Debug.LogWarning($"Cannot add velocity to spawned object {child.name} because parent {relativeVelocityProvider.name} has no Rigidbody2D component.  Disabling addSpawnVelocity.");
                    this.addSpawnVelocity = false;
                }
            }

            spawnedRigidBody.velocity = velocity;
        }

        public Transform Spawn(Transform relativePositionProvider, Transform childPrefab, Transform parent = null)
        {
            if (PauseManager.IsPaused && !this.allowSpawnWhenPaused)
            {
                return null;
            }
            
            var spawnedChild = Object.Instantiate(childPrefab);
            this.SetChildSpawnPosition(relativePositionProvider, spawnedChild);
            this.SetChildSpawnVelocity(relativePositionProvider, spawnedChild);

            // Apply extra scale.
            var spawnedTransform = spawnedChild.transform;
            var localScale = spawnedTransform.localScale;
            localScale.x *= this.extraScale.x;
            localScale.y *= this.extraScale.y;
            localScale.z *= this.extraScale.z;
            spawnedTransform.localScale = localScale;
            
            if (parent)
            {
                spawnedChild.parent = parent;
            }
            
            return spawnedChild;
        }
    }
}