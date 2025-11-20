namespace RyanMillerGameCore.Items
{
    using UnityEngine;
    using Utilities;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AutoRegisterID))]
    public class Item : MonoBehaviour, IPoolable, IHasID
    {
        [Tooltip("The unique identifier for this item type")]
        public ID identifier;

        public ID GetID() => identifier;

        public virtual void OnAcquire()
        {
            // Reset state if needed
            // e.g. enable visual effects, reset timers, etc.
        }

        public virtual void OnRelease()
        {
            // Clean up before pooling
            // e.g. disable effects, stop sounds, reset transforms
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}