using Scripts.Helpers;
using Scripts.UI;

namespace Scripts.Gameplay.AI
{
    public class GoToTeleport : GoToBase
    {
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!DestinationProvider) return;
            if (!DestinationProvider.CurrentDestination.HasValue) return;
            if (!DestinationProvider.IsCloseEnoughToDestination)
            {
                this.transform.position = OffsetProviderTools.GetTotalOffset(this.gameObject) 
                                          + DestinationProvider.CurrentDestination.Value.ToVector3WithZ(transform.position.z);
            }
        }
    }
}