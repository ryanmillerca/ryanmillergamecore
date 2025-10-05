namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;
    using Interactions;

    public class LookAtSensedTransform : ColliderSensor
    {
        private Transform target;
        private bool look = false;
        [SerializeField] private float turnSpeed = 5;

        protected override void ItemEnteredTrigger(Collider item)
        {
            look = true;
            target = item.transform;
        }

        protected override void ItemExitedTrigger(Collider item)
        {
            look = false;
            target = null;
        }

        private void Update()
        {
            if (!look)
            {
                return;
            }

            Vector3 direction = target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }
    }
}