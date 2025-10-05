namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;
    using System;

    public class AlignToDirection : MonoBehaviour
    {
        [SerializeField] private float minDeltaThreshold = 0.0001f;
        [SerializeField] private float rotationSpeedFacing = 5f;

        [NonSerialized] private Vector3 _lastPosition;

        private void Start()
        {
            _lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            AlignToMovementDirection();
        }

        private void AlignToMovementDirection()
        {
            Vector3 deltaPosition = transform.position - _lastPosition;
            if (deltaPosition.sqrMagnitude > minDeltaThreshold)
            {
                Quaternion targetDirectionRotation = Quaternion.LookRotation(deltaPosition.normalized, transform.up);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetDirectionRotation, rotationSpeedFacing * Time.fixedDeltaTime);
            }
            _lastPosition = transform.position;
        }
    }
}