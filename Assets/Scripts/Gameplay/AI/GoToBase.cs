using System.Linq;
using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class GoToBase : MonoBehaviour
    {
        public DestinationProvider DestinationProvider
        {
            //get => this._destinationProvider;
            get => this._destinationProvider;
            set
            {
                if (value == this._destinationProvider) return;

                // Clear old destination provider
                if (this._destinationProvider)
                {
                    this._destinationProvider.onDestinationChanged.RemoveListener(OnDestinationChanged);
                    this._destinationProvider.onDestinationReached.RemoveListener(OnDestinationReached);
                    this._destinationProvider = null;
                }

                // Init the new destination provider
                if (value)
                {
                    if (!value.enabled)
                    {
                        Debug.LogWarning($"Setting disabled {value} as destination provider");
                    }
                    this._destinationProvider = value;
                    this._destinationProvider.onDestinationChanged.AddListener(OnDestinationChanged);
                    this._destinationProvider.onDestinationReached.AddListener(OnDestinationReached);
                    OnDestinationChanged(this._destinationProvider.CurrentDestination);
                }
            }
        }

        protected Rigidbody2D cachedRigidbody2D;
        private DestinationProvider _destinationProvider;

        public void UpdateDestinationProvider()
        {
            // If it ain't broke, don't fix it.
            if (this._destinationProvider && this._destinationProvider.enabled) return;

            var options = this.GetComponents<DestinationProvider>()
                .Where(mb => mb.enabled);

            var firstValidOrNull = options.FirstOrDefault();
            this.DestinationProvider = firstValidOrNull;
        }

        protected virtual void FixedUpdate()
        {
            UpdateDestinationProvider(); // called to ensure that the destination provider is set
        }

        protected virtual void OnDestinationReached(Vector2 destination)
        {

        }

        public virtual void OnDestinationChanged(Vector2? newDestination)
        {
        }

        protected virtual void OnEnable()
        {
            cachedRigidbody2D = GetComponent<Rigidbody2D>();
        }
    }
}