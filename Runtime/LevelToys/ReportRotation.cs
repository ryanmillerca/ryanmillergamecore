namespace RyanMillerGameCore.LevelToys
{
    using UnityEngine;
    using System;
    using Utilities;

    public class ReportRotation : MonoBehaviour, IFloatValueProvider
    {
        public event Action<float> ValueChanged;
        public float SpinValue => spinValue;
        public float CurrentValue => spinValue;
        
        [Header("Rotation Tracking")]
        [SerializeField] private float spinValue;

        [Tooltip("Should the script actively detect and report rotation?")]
        [SerializeField] private bool detecting = true;

        [Tooltip("Which local axis to track for rotation changes.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;

        [Header("Clamping")]
        [Tooltip("Choose how to constrain the spin value.")]
        [SerializeField] private ClampMode clampMode = ClampMode.Normal;

        private Vector3 _previousEulerAngles;
        
        private void Awake()
        {
            _previousEulerAngles = transform.localEulerAngles;
        }

        private void Update()
        {
            if (!detecting)
            {
                return;
            }

            float delta = CalculateDelta();
            ApplyRotationChange(delta);
            ApplyClamping();
            UpdatePreviousRotation();
        }

        private float CalculateDelta()
        {
            Quaternion previous = Quaternion.Euler(_previousEulerAngles);
            Quaternion current = transform.localRotation;

            Quaternion deltaRotation = Quaternion.Inverse(previous) * current;
            Vector3 deltaEuler = deltaRotation.eulerAngles;

            // Normalize to [-180, 180]
            deltaEuler.x = NormalizeAngle(deltaEuler.x);
            deltaEuler.y = NormalizeAngle(deltaEuler.y);
            deltaEuler.z = NormalizeAngle(deltaEuler.z);

            return rotationAxis switch
            {
                RotationAxis.X => deltaEuler.x,
                RotationAxis.Y => deltaEuler.y,
                RotationAxis.Z => deltaEuler.z,
                _ => 0f
            };
        }

        private void ApplyRotationChange(float delta)
        {
            spinValue += delta;
        }

        private void ApplyClamping()
        {
            switch (clampMode)
            {
                case ClampMode.PositiveOnly:
                    spinValue = Mathf.Clamp(spinValue, 0f, Mathf.Infinity);
                    break;
                case ClampMode.NegativeOnly:
                    spinValue = Mathf.Clamp(spinValue, -Mathf.Infinity, 0f);
                    break;
                // ClampMode.Normal: do nothing
            }

            RaiseChange();
        }

        private void UpdatePreviousRotation()
        {
            _previousEulerAngles = transform.localEulerAngles;
        }

        private float NormalizeAngle(float angle)
        {
            return (angle > 180f) ? angle - 360f : angle;
        }

        private void RaiseChange()
        {
            ValueChanged?.Invoke(spinValue);
        }
    }

    public enum ClampMode
    {
        Normal = 0,
        PositiveOnly = 1,
        NegativeOnly = 2
    }

    public enum RotationAxis
    {
        X = 0,
        Y = 1,
        Z = 2
    }
}