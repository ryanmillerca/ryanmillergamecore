namespace RyanMillerGameCore.Character
{
    using System;
    using UnityEngine;
    using UnityEngine.AI;
    using SMB;

    /// <summary>
    /// Pathfinding controller that can drive characters via movement input or a forceful/cinematic mode.
    /// Automatically avoids driving into target colliders by stopping short (speakerRadius + targetRadius + padding).
    /// Falls back to exact target position when no solid colliders are found in the target's hierarchy.
    /// </summary>
    public class CharacterPathfind : MonoBehaviour
    {
        #region Events

        public event Action<float> PathCompleted;
        public event Action<float, float> PathTakingTooLong;
        public event Action<string> PathFailed;

        #endregion

        #region Serialized Fields

        [Tooltip("Character-wide references container.")] [SerializeField]
        private CharacterReferences references;

        [Tooltip("Meters to consider arrived at the final goal.")] [SerializeField]
        private float arrivalRadius = 0.4f;

        [Tooltip("Meters to advance to the next corner.")] [SerializeField]
        private float cornerAdvanceRadius = 0.5f;

        [Tooltip("Seconds between path recalculations while moving.")] [SerializeField]
        private float pathRecalcCooldown = 0.2f;

        [Tooltip("Radius used to project the target onto NavMesh.")] [SerializeField]
        private float targetSampleRadius = 1.0f;

        [Header("Collider Clearance")]
        [Tooltip("Extra padding to keep from bumping into colliders at destination.")]
        [SerializeField]
        private float destinationPadding = 0.1f;

        [Header("Timeouts (Simple)")]
        [Tooltip("Warn that navigation is taking a while after this many seconds. 0 = Auto based on path length.")]
        [SerializeField]
        private float warnAfterSeconds = 6f;

        [Tooltip("Give up after this many seconds of navigating.")] [SerializeField]
        private float giveUpAfterSeconds = 12f;

        [Header("Stuck Detection")]
        [Tooltip("If movement over 'stuckWindowSeconds' < this, it counts as stuck for that window (meters).")]
        [SerializeField]
        private float stuckDistanceThreshold = 0.15f;

        [Tooltip("Seconds per window for stuck detection.")] [SerializeField]
        private float stuckWindowSeconds = 1.5f;

        [Tooltip("After accumulating this much 'stuck' time (in windows), fail.")] [SerializeField]
        private float stuckFailAfterSeconds = 3.0f;

        [Header("Failure & Debug")] [Tooltip("Continuous time with invalid/partial path before fail.")] [SerializeField]
        private float invalidPathFailSeconds = 1.0f;

        [Tooltip("Draw lines for the current path and intended input.")] [SerializeField]
        private bool debugDrawPath = false;

        #endregion

        #region Private Fields

        private NavMeshPath _path;
        private int _currentCornerIndex = 1;
        private int _lastCornerCount = 0;
        private float _pathRecalcTimer;
        private bool _isNavigating;
        private float _startTime;
        private float _expectedDuration;
        private bool _tooLongFired;
        private Vector3 _stuckWindowStartPos;
        private float _stuckWindowStartTime;
        private float _stuckAccumulatedTime;
        private float _invalidPathAccumulatedTime;

        private Transform _explicitTargetTransform;
        private bool _forceful;

        // Debug/telemetry of current goal
        private float _goalStopDistance;
        private Vector3 _goalApproachPoint;

        #endregion

        #region Types (Options)

        /// <summary>
        /// Options to control path start behavior.
        /// </summary>
        public struct StartOptions
        {
            public Transform targetTransform;
            public bool forceful;

            public static StartOptions Forceful(Transform t)
                => new StartOptions { targetTransform = t, forceful = true };
        }

        #endregion

        #region Unity

        private void OnEnable()
        {
            if (references == null)
            {
                references = GetComponentInChildren<CharacterReferences>();
            }

            if (references == null)
            {
                Debug.LogError($"{nameof(CharacterPathfind)}: 'references' not assigned.", this);
            }
        }

        private void OnValidate()
        {
            arrivalRadius = Mathf.Max(0.01f, arrivalRadius);
            cornerAdvanceRadius = Mathf.Max(0.01f, cornerAdvanceRadius);

            pathRecalcCooldown = Mathf.Clamp(pathRecalcCooldown, 0.02f, 2f);
            targetSampleRadius = Mathf.Clamp(targetSampleRadius, 0.05f, 5f);
            destinationPadding = Mathf.Clamp(destinationPadding, 0f, 1f);

            warnAfterSeconds = Mathf.Max(0f, warnAfterSeconds);
            giveUpAfterSeconds = Mathf.Max(1f, giveUpAfterSeconds);
            if (warnAfterSeconds > 0f && giveUpAfterSeconds < warnAfterSeconds + 0.5f)
                giveUpAfterSeconds = warnAfterSeconds + 0.5f;

            stuckDistanceThreshold = Mathf.Clamp(stuckDistanceThreshold, 0.01f, 1f);
            stuckWindowSeconds = Mathf.Clamp(stuckWindowSeconds, 0.2f, 10f);
            stuckFailAfterSeconds = Mathf.Clamp(stuckFailAfterSeconds, 0.2f, 30f);
            invalidPathFailSeconds = Mathf.Clamp(invalidPathFailSeconds, 0.05f, 10f);
        }

        private void Update()
        {
            if (!_isNavigating) return;

            Transform effectiveTarget = _explicitTargetTransform != null
                ? _explicitTargetTransform
                : (references != null && references.characterBrain != null ? references.characterBrain.Target : null);

            if (effectiveTarget == null)
            {
                Fail("invalid");
                return;
            }

            Vector3 startPos = transform.position;

            // Periodic path recalculation (recompute approach point every cycle)
            _pathRecalcTimer -= Time.deltaTime;
            if (_pathRecalcTimer <= 0f)
            {
                Vector3 targetPos = ComputeApproachPoint(effectiveTarget, startPos, out _goalStopDistance,
                    out _goalApproachPoint);
                RecalculatePath(startPos, targetPos);
                RecalculateExpectedDuration();
                _pathRecalcTimer = pathRecalcCooldown;
            }

            if (!HasUsablePath())
            {
                _invalidPathAccumulatedTime += Time.deltaTime;
                if (_invalidPathAccumulatedTime >= invalidPathFailSeconds)
                {
                    Fail("invalid");
                    return;
                }

                return;
            }

            _invalidPathAccumulatedTime = 0f;

            // Clamp index when topology changes
            if (_path.corners.Length != _lastCornerCount)
            {
                _currentCornerIndex = Mathf.Clamp(_currentCornerIndex, 1, _path.corners.Length - 1);
                _lastCornerCount = _path.corners.Length;
            }

            // Arrival check at the last corner (our approach point)
            Vector3 final = _path.corners[_path.corners.Length - 1];
            float arrival = Mathf.Max(arrivalRadius, 0.05f);
            if ((final - startPos).sqrMagnitude <= arrival * arrival)
            {
                Complete();
                return;
            }

            // Advance to next corner when close enough
            Vector3 next = _path.corners[_currentCornerIndex];
            if ((next - startPos).sqrMagnitude < cornerAdvanceRadius * cornerAdvanceRadius
                && _currentCornerIndex < _path.corners.Length - 1)
            {
                _currentCornerIndex++;
                next = _path.corners[_currentCornerIndex];
            }

            // Movement command
            Vector3 moveDir = next - startPos;
            moveDir.y = 0f;
            if (moveDir.sqrMagnitude <= 0.0001f) moveDir = Vector3.zero;

            if (references != null && references.movement != null)
            {
                if (_forceful)
                {
                    // Decisive cinematic movement (MovePosition) using CharacterMovement's forceful method
                    references.movement.MoveForcefulTowards(next, Time.deltaTime);
                }
                else
                {
                    // Gameplay style (forces)
                    references.movement.Move(moveDir);
                }
            }

            // Timers/warnings/fails
            float runTime = Time.time - _startTime;

            float autoWarn = Mathf.Clamp(_expectedDuration * 1.5f, 2f, 10f);
            float warnAt = (warnAfterSeconds > 0f) ? warnAfterSeconds : autoWarn;

            if (!_tooLongFired && runTime >= warnAt)
            {
                _tooLongFired = true;
                PathTakingTooLong?.Invoke(runTime, _expectedDuration);
            }

            if (runTime >= giveUpAfterSeconds)
            {
                Fail("timeout");
                return;
            }

            // Stuck detection window
            float windowAge = Time.time - _stuckWindowStartTime;
            if (windowAge >= stuckWindowSeconds)
            {
                float moved = Vector3.Distance(_stuckWindowStartPos, transform.position);
                bool stuckNow = moved < stuckDistanceThreshold;

                if (stuckNow) _stuckAccumulatedTime += windowAge;
                else _stuckAccumulatedTime = 0f;

                _stuckWindowStartTime = Time.time;
                _stuckWindowStartPos = transform.position;

                if (_stuckAccumulatedTime >= stuckFailAfterSeconds)
                {
                    Fail("stuck");
                    return;
                }
            }

            // Debug draw
            if (debugDrawPath && _path != null && _path.corners != null)
            {
                for (int i = 0; i < _path.corners.Length - 1; i++)
                {
                    Debug.DrawLine(_path.corners[i], _path.corners[i + 1], Color.green);
                }

                Debug.DrawRay(transform.position, moveDir, Color.cyan);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Back-compat start: behaves as before (no forceful motion).
        /// </summary>
        public void StartPath(Transform targetTransform = null)
        {
            StartPath(new StartOptions
            {
                targetTransform = targetTransform,
                forceful = false
            });
        }

        /// <summary>
        /// Preferred start with options (use for dialog-driven movement to enable forceful).
        /// Clearance is automatic based on colliders on the target hierarchy.
        /// </summary>
        public void StartPath(StartOptions options)
        {
            if (references == null)
            {
                Fail("invalid");
                return;
            }

            Transform effective = options.targetTransform != null
                ? options.targetTransform
                : (references.characterBrain != null ? references.characterBrain.Target : null);

            if (effective == null)
            {
                Fail("no target");
                return;
            }

            _explicitTargetTransform = options.targetTransform != null ? options.targetTransform : null;
            _forceful = options.forceful;

            _path ??= new NavMeshPath();

            if (references.movement != null)
            {
                references.movement.CanMove(true);
            }

            _currentCornerIndex = 1;
            _lastCornerCount = 0;
            _pathRecalcTimer = 0f;

            _isNavigating = true;
            _startTime = Time.time;
            _tooLongFired = false;

            _stuckWindowStartPos = transform.position;
            _stuckWindowStartTime = Time.time;
            _stuckAccumulatedTime = 0f;
            _invalidPathAccumulatedTime = 0f;

            Vector3 start = transform.position;
            Vector3 targetPos = ComputeApproachPoint(effective, start, out _goalStopDistance, out _goalApproachPoint);
            RecalculatePath(start, targetPos);
            RecalculateExpectedDuration();
        }

        public void Cancel()
        {
            _isNavigating = false;
            if (references != null && references.movement != null)
            {
                references.movement.Move(Vector3.zero);
            }
        }

        #endregion

        #region Path Computation & Helpers

        private Vector3 ComputeApproachPoint(Transform target, Vector3 from, out float stopDist,
            out Vector3 rawApproach)
        {
            float speakerRadius = GetApproxSpeakerRadius();

            Collider solidCollider = FindHierarchySolidCollider(target);

            if (solidCollider)
            {
                // Closest point on target collider bounds to the speaker
                Vector3 closest = solidCollider.ClosestPoint(from);

                // Direction from speaker toward that point
                Vector3 dir = (closest - from);
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.001f)
                {
                    dir = target.forward;
                }
                dir.Normalize();

                // Back off by speaker radius + padding
                stopDist = speakerRadius + destinationPadding;
                rawApproach = closest - dir * stopDist;

                if (NavMesh.SamplePosition(rawApproach, out var hit, targetSampleRadius, NavMesh.AllAreas))
                {
                    return hit.position;
                }

                return rawApproach;
            }

            // No colliders, head to transform position
            stopDist = speakerRadius + destinationPadding;
            rawApproach = target.position;

            if (NavMesh.SamplePosition(rawApproach, out var hit2, targetSampleRadius, NavMesh.AllAreas))
            {
                return hit2.position;
            }

            return rawApproach;
        }

        /// <summary>
        /// Finds a non-trigger collider on the target, its children, or its parent hierarchy.
        /// Returns null if none found.
        /// </summary>
        private Collider FindHierarchySolidCollider(Transform t)
        {
            // is the collider on the transform and NOT a trigger?
            Collider c = t.GetComponent<Collider>();
            if (c && !c.isTrigger)
            {
                return c;
            }

            // is the collider in the children and NOT a trigger?
            var child = t.GetComponentInChildren<Collider>();
            if (child && !child.isTrigger)
            {
                return child;
            }

            // is the collider in the parents and NOT a trigger?
            var parent = t.GetComponentInParent<Collider>();
            if (parent && !parent.isTrigger)
            {
                return parent;
            }

            return null;
        }

        private float GetApproxSpeakerRadius()
        {
            // approximate radius as half the min dimension in XZ
            var size = references.mainCollider.bounds.size;
            return 0.5f * Mathf.Min(size.x, size.z);
        }

        private void RecalculatePath(Vector3 start, Vector3 target)
        {
            _path ??= new NavMeshPath();

            bool ok = NavMesh.CalculatePath(start, target, NavMesh.AllAreas, _path);
            if (!ok || _path.corners == null)
            {
                _lastCornerCount = 0;
                return;
            }

            if (_path.corners.Length > 1)
                _currentCornerIndex = Mathf.Clamp(_currentCornerIndex, 1, _path.corners.Length - 1);
            else
                _currentCornerIndex = 0;

            _lastCornerCount = _path.corners.Length;
        }

        private bool HasUsablePath()
        {
            if (_path == null) return false;
            if (_path.corners == null || _path.corners.Length < 2) return false;
            if (_path.status == NavMeshPathStatus.PathInvalid) return false;
            if (_path.status == NavMeshPathStatus.PathPartial) return false;
            return _currentCornerIndex >= 1 && _currentCornerIndex < _path.corners.Length;
        }

        private void RecalculateExpectedDuration()
        {
            float moveSpeed = 100f;
            if (references != null && references.movement != null && references.movement.maxSpeed > 0f)
            {
                moveSpeed = references.movement.maxSpeed;
                if (_forceful && references.movement.forcefulSpeed > 0f)
                    moveSpeed = references.movement.forcefulSpeed;
            }

            if (!HasUsablePath())
            {
                _expectedDuration = 5f;
                return;
            }

            float length = 0f;
            for (int i = 0; i < _path.corners.Length - 1; i++)
            {
                length += Vector3.Distance(_path.corners[i], _path.corners[i + 1]);
            }

            _expectedDuration = Mathf.Max(0.1f, length / Mathf.Max(0.01f, moveSpeed));
        }

        private void Complete()
        {
            _isNavigating = false;
            if (references != null && references.movement != null)
            {
                references.movement.Move(Vector3.zero);
            }

            PathCompleted?.Invoke(Time.time - _startTime);
        }

        private void Fail(string reason)
        {
            _isNavigating = false;
            if (references != null && references.movement != null)
            {
                references.movement.Move(Vector3.zero);
            }

            PathFailed?.Invoke(reason);
        }

        #endregion
    }
}