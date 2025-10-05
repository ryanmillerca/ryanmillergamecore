namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;
    using System;

    /// <summary>
    /// Aligns an object's tilt/rotation to the ground it's travelling across
    /// Uses Raycast Hit's Collider Normal to determine the rotation 
    /// </summary>
    public class AlignToGround : MonoBehaviour
    {
        [Header("Behaviour")] 
        [SerializeField] private bool alignToGround = true;
        [SerializeField] private bool lowerToGround = false;
        [SerializeField] private bool freezeYRotation = true;
        [SerializeField] private UpdateMode runMode = UpdateMode.FixedUpdate;

        [Header("References")] [SerializeField]
        private LayerMask groundLayer;

        [SerializeField] private Collider _collider;

        [Header("Parameters")] 
        
        [SerializeField] private float raycastDistance = 2f;
        [SerializeField] private float rotationSpeedTilt = 5f;
        [NonSerialized] private Quaternion _targetRotation;
        [NonSerialized] private bool _hasCollider;
        [NonSerialized] private RaycastHit _hit;

        private Collider Collider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }

                _hasCollider = _collider != null;
                return _collider;
            }
        }

        private void Update()
        {
            if (runMode == UpdateMode.Update)
            {
                DoAlignment();
            }
        }
        
        private void FixedUpdate()
        {
            if (runMode == UpdateMode.FixedUpdate)
            {
                DoAlignment();
            }
        }

        private void DoAlignment(bool tweenRotation = true)
        {
            AlignTransformToGround();
            LowerToGround();
            FreezeYRotation();
            if (tweenRotation)
            {
                transform.rotation =
                    Quaternion.Lerp(transform.rotation, _targetRotation, Time.deltaTime * rotationSpeedTilt);
            }
            else
            {
                transform.rotation = _targetRotation;
            }
        }

        private void Reset()
        {
            groundLayer = 1 << LayerMask.NameToLayer("Ground");
        }

        private void FreezeYRotation()
        {
            if (!freezeYRotation)
            {
                return;
            }

            Vector3 localEuler = transform.localEulerAngles;
            localEuler.y = 0;
            transform.localEulerAngles = localEuler;
        }

        [ContextMenu("Align to Ground")]
        private void Start()
        {
            _collider = Collider;
            _targetRotation = transform.rotation;
            if (runMode == UpdateMode.Start)
            {
                DoAlignment(false);
            }
        }

        private void AlignTransformToGround()
        {
            if (!alignToGround)
            {
                return;
            }

            if (Physics.Raycast(transform.position, Vector3.down, out _hit, raycastDistance, groundLayer))
            {
                Vector3 groundNormal = _hit.normal;
                Quaternion targetAlignRotation =
                    Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
                _targetRotation = Quaternion.Slerp(_targetRotation, targetAlignRotation,
                    rotationSpeedTilt * Time.fixedDeltaTime);
            }
        }

        private void LowerToGround()
        {
            if (lowerToGround == false)
            {
                return;
            }

            if (_hit.collider != null)
            {
                if (_hasCollider)
                {
                    var bounds = _collider.bounds;
                    Vector3 bottomCenter = bounds.center - new Vector3(0, bounds.extents.y, 0);
                    float distanceToGround = _hit.distance - (bounds.center.y - bottomCenter.y);
                    transform.position -= new Vector3(0, distanceToGround, 0);
                }
                else
                {
                    transform.position -= new Vector3(0, _hit.distance, 0);
                }
            }
        }
    }
    
    public enum UpdateMode
    {
        Start,
        Update,
        FixedUpdate,
        LateUpdate
    }
}