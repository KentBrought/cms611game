using System;
using Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Gameplay.PlayerInput
{
    /*
     *  Handles player inputs by auto-subscribing to Move and Fire events in code.
     */
    public class PlayerMovementBase : InputHandlerBase
    {
        protected Rigidbody2D cachedRigidbody2D;
 
        protected override void Start() 
        {
            base.Start();
            this.cachedRigidbody2D = this.GetComponent<Rigidbody2D>();
            this.cachedHitPoints = this.GetComponent<HitPoints>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // If Move or Fire is null, there's a problem somewhere and we want an exception to tell us about it.
            this.Move.Enable();
            
            var spawner = GetComponent<SpawnOnCommand>();
            if (spawner)
            {
                this.Fire.started += spawner.SpawnPrefabNow;
                this.Fire.Enable();
            }
            

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            var spawner = GetComponent<SpawnOnCommand>();
            if (spawner && null != this.Fire)
            {
                // We're cleaning up after ourselves.
                //  Hmm, would it be better to clear ALL events from the Player action map entirely?
                this.Fire.started -= spawner.SpawnPrefabNow;
            }

            // If Move is null, no need to disable it.
            //  This can happen when exiting play mode in editor.
            this.Move?.Disable();
            this.Fire?.Disable();
            
        }

        protected virtual void FixedUpdate()
        {
            if (null == this.Move)
            {
                this._lastMoveInput = Vector2.zero;
                return;
            }
            
            this._lastMoveInput = this.Move?.ReadValue<Vector2>();
        }

        private Vector2? _lastMoveInput;

        
        protected Vector2 GetMoveInput()
        {
            if (!this.ShouldProcessInput) return Vector2.zero;
            return this._lastMoveInput ?? Vector2.zero;
        }
    }
}