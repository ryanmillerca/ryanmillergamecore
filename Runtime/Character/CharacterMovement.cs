namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    using System;

    /// <summary>
    /// Handles physics-based character movement and rotation logic.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMovement : MonoBehaviour, IMovable
    {
        #region Serialized Fields

        [Tooltip("Maximum speed this will move (from its own force)")]
        public float maxSpeed = 5f;

        [Tooltip("How quickly this object will reach max speed")]
        public float acceleration = 10f;

        [Tooltip("How quickly the object slows down when not moving")]
        public float deceleration = 10f;

        [Header("Dialog/Forceful Move")]
        [Tooltip("Max speed used by forceful/MovePosition pathing (dialog lines). Defaults to maxSpeed if 0.")]
        public float forcefulSpeed = 0f;

        [Tooltip("If true, a tiny snap is allowed when nearly at the target and blocked (dialog lines).")]
        public bool allowTinyWarpNearGoal = true;

        [Tooltip("Max distance for the tiny warp when nearly at goal (meters).")]
        public float tinyWarpDistance = 0.1f;

        #endregion

        #region Private Fields

        private Rigidbody _rb;
        private Vector3 _targetVelocity;
        private bool _canMove = false;
        private RotateFromMotor _rotateFromMotor;

        #endregion

        #region Events

        public event Action<float> OnVelocityApplied;
        public event Action<Vector3> OnMoveInDirection;

        #endregion

        #region Unity Events

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            Character character = GetComponent<Character>();
            if (character)
            {
                character.OnKnockedBack += WasKnockedBack;
            }
        }

        private void OnDisable()
        {
            Character character = GetComponent<Character>();
            if (character)
            {
                character.OnKnockedBack -= WasKnockedBack;
            }
        }

        #endregion

        #region Monobehaviour Methods

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        #endregion

        #region IMovable Implementation

        /// <summary>
        /// Receives movement input and applies it to the character.
        /// </summary>
        public void Move(Vector3 input)
        {
            if (!_canMove)
            {
                return;
            }

            OnMoveInDirection?.Invoke(input);
            _targetVelocity = input * maxSpeed;
        }

        public void Teleport(Vector3 position)
        {
            _rb.position = position;
            _targetVelocity = Vector3.zero;
            transform.position = position;
        }
        
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _rb.position = position;
            _targetVelocity = Vector3.zero;
            RotateFromMotor.SetRotation(rotation);
            transform.position = position;
        }

        public void CanMove(bool can)
        {
            _canMove = can;
        }

        public Vector3 Position()
        {
            return transform.position;
        }

        public void PushForward(float amount, bool resetVelocityFirst = true)
        {
            if (resetVelocityFirst)
            {
                _rb.linearVelocity = Vector3.zero;
            }

            Vector3 force = ForwardFacing * amount;
            force.y = 0;
            _rb.AddForce(force, ForceMode.Force);
        }

        #endregion

        #region Movement Logic

        /// <summary>
        /// Applies movement forces and clamps horizontal velocity.
        /// </summary>
        public void ApplyMovement()
        {
            if (!_canMove)
            {
                _targetVelocity = Vector3.zero;
            }

            // Skip applying force if there's no movement input
            if (_targetVelocity.sqrMagnitude > 0.001f)
            {
                Vector3 velocityDifference = _targetVelocity - _rb.linearVelocity;
                velocityDifference.y = 0;

                float forceMultiplier = _targetVelocity.magnitude > 0 ? acceleration : deceleration;
                _rb.AddForce(velocityDifference * forceMultiplier, ForceMode.Acceleration);
            }

            // Clamp horizontal velocity
            Vector3 horizontalVelocity = _rb.linearVelocity;
            horizontalVelocity.y = 0;

            if (horizontalVelocity.magnitude > maxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                _rb.linearVelocity = new Vector3(horizontalVelocity.x, _rb.linearVelocity.y, horizontalVelocity.z);
            }

            OnVelocityApplied?.Invoke(_rb.linearVelocity.magnitude / Mathf.Max(0.001f, maxSpeed));
        }

        public void WasKnockedBack(Vector3 knockback)
        {
            knockback.y = 0;
            _rb.AddForce(knockback, ForceMode.Impulse);
        }

        #endregion

        #region Forceful Move (Dialog)

        /// <summary>
        /// Forceful, resolute movement toward a world-space point using Rigidbody.MovePosition
        /// (ignores forces/accel; better for cinematic dialog motion).
        /// Optionally performs a tiny warp when almost there but blocked.
        /// Call from Update with a world target (e.g., path corner).
        /// </summary>
        public void MoveForcefulTowards(Vector3 worldTarget, float dt)
        {
            if (!_canMove)
            {
                return;
            }

            float spd = (forcefulSpeed > 0f) ? forcefulSpeed : Mathf.Max(0.01f, maxSpeed);
            Vector3 pos = _rb.position;
            Vector3 to = worldTarget - pos;
            to.y = 0f;

            float dist = to.magnitude;
            if (dist < 0.0001f)
            {
                _targetVelocity = Vector3.zero;
                return;
            }

            Vector3 step = to.normalized * spd * dt;

            if (step.magnitude >= dist)
            {
                // Reaching this frame — either MovePosition to exact target, or tiny warp if blocked.
                Vector3 dest = worldTarget;
                if (allowTinyWarpNearGoal && dist <= tinyWarpDistance)
                {
                    // Commit to exact end even if something minor blocks (cinematic authority).
                    _rb.position = dest; // direct set is OK for tiny snap; keeps continuity
                }
                else
                {
                    _rb.MovePosition(dest);
                }

                _targetVelocity = Vector3.zero;
                return;
            }

            _rb.MovePosition(pos + step);
            _targetVelocity = step / Mathf.Max(0.0001f, dt); // for any listeners
            OnMoveInDirection?.Invoke(_targetVelocity);
        }

        #endregion

        #region Rotation

        public float CharacterViewRotation
        {
            get { return RotateFromMotor.transform.localEulerAngles.y; }
            set {
                RotateFromMotor.SetRotation(Quaternion.Euler(0, value, 0));
            }
        }

        public Transform ForwardTransform
        {
            get { return RotateFromMotor.transform; }
        }

        /// <summary>
        /// Rotates the character to face a given world position.
        /// </summary>
        public void LookAt(Vector3 position)
        {
            Vector3 direction = position - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion lookRotation = Quaternion.LookRotation(direction);

            if (RotateFromMotor != null)
            {
                RotateFromMotor.SetRotation(lookRotation);
            }
            else
            {
                transform.rotation = lookRotation;
            }
        }

        private Vector3 ForwardFacing => RotateFromMotor.transform.forward;

        private RotateFromMotor RotateFromMotor
        {
            get
            {
                if (_rotateFromMotor == null)
                {
                    _rotateFromMotor = GetComponentInChildren<RotateFromMotor>();
                }

                return _rotateFromMotor;
            }
        }

        #endregion
    }
}