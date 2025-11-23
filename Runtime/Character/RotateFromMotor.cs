namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    using Utilities;
    using System;

    public class RotateFromMotor : MonoBehaviour
    {
        [SerializeField] private float rotationOffset = 90;
        [SerializeField] private float minThreshold = 0.001f;
        [SerializeField] private float lerpSpeed = 6;
        [SerializeField] private UpdateMode runMode = UpdateMode.Update;
        [SerializeField] private bool allowTilting = true;
        
        [NonSerialized] private Quaternion _targetRotation;
        [NonSerialized] private CharacterMovement _characterMovement;

        
        private void Awake() {
            if (allowTilting) {
                _targetRotation = transform.localRotation;
            }
            else {
                _targetRotation = transform.rotation;
            }
        }
        
        private void OnEnable()
        {
            _characterMovement = GetComponentInParent<CharacterMovement>();
            _characterMovement.OnMoveInDirection += OnMoveInDirection;
        }

        private void OnDisable()
        {
            _characterMovement.OnMoveInDirection -= OnMoveInDirection;
        }

        private void OnMoveInDirection(Vector3 inputDirection)
        {
            if (inputDirection.sqrMagnitude > minThreshold)
            {
                float angle = Mathf.Atan2(inputDirection.z, inputDirection.x);
                float angleDegrees = -angle * Mathf.Rad2Deg;
                _targetRotation = Quaternion.Euler(0, angleDegrees + rotationOffset, 0);
            }
        }

        private void Update()
        {
            if (runMode == UpdateMode.Update)
            {
                Execute();
            }
        }

        private void FixedUpdate()
        {
            if (runMode == UpdateMode.FixedUpdate)
            {
                Execute();
            }
        }
        
        private void LateUpdate()
        {
            if (runMode == UpdateMode.LateUpdate)
            {
                Execute();
            }
        }

        private void Execute()
        {
            if (lerpSpeed > 0)
            {
                if (allowTilting) {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, _targetRotation, DeltaTimeValue() * lerpSpeed);
                }
                else {
                    transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, DeltaTimeValue() * lerpSpeed);
                }
            }
            else {
                transform.rotation = _targetRotation;
            }
        }

        private float DeltaTimeValue()
        {
            if (runMode == UpdateMode.FixedUpdate)
            {
                return Time.fixedDeltaTime;
            }
            return Time.deltaTime;
        }

        public void SetRotation(Quaternion targetRot, bool immediately = false)
        {
            _targetRotation = targetRot;
            if (immediately || lerpSpeed <= 0)
            {
                if (allowTilting) {
                    transform.localRotation = _targetRotation;
                }
                else {
                    transform.rotation = _targetRotation;
                }
            }
        }
    }
}