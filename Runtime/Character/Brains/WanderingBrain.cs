namespace RyanMillerGameCore.Character
{
    using System;
    using UnityEngine;

    public class WanderingBrain : CharacterBrain {
        [SerializeField] private WanderType wanderType;
        [SerializeField] private WanderDirection wanderDirection;
        [SerializeField] private float wanderTimeMin = 2f;
        [SerializeField] private float wanderTimeMax = 5f;
        [SerializeField] private float wanderInterval = 3f;
        [SerializeField] private float wanderRadius = 5f;
        [Tooltip("Anti-stuck tech: Minimum movement to consider progress")]
        [SerializeField] private float stuckThreshold = 0.1f; 
        [Tooltip("Anti-stuck tech: Time to wait before considering stuck")]
        [SerializeField] private float stuckTime = 2f;
        
        private enum WanderType { NavMesh, DirectionWalk }
        private enum WanderState { Wandering, Idling }
        private enum WanderDirection { RandomInCircle, RandomCartesian }
        private WanderState _wanderState;

        [NonSerialized] private float _timer;
        [NonSerialized] private Vector3 _direction;
        [NonSerialized] private Vector3 _targetDirection;
        [NonSerialized] private Vector3 _lastPosition;
        [NonSerialized] private float _stuckTimer = 0f;
        
        
        private static readonly Vector3[] CartesianDirections = {
            new Vector3(0, 0, 1),   // Forward
            new Vector3(1, 0, 0),   // Right
            new Vector3(0, 0, -1),  // Backward
            new Vector3(-1, 0, 0)   // Left
        };

        protected override void Awake()
        {
            base.Awake();
            StartWandering();
        }

        #pragma warning disable CS0252

        private void StartWandering()
        {
            _timer = UnityEngine.Random.Range(wanderTimeMin, wanderTimeMax);
            _targetDirection = GetNewWanderDirection();
            _direction = _targetDirection;
            _wanderState = WanderState.Wandering;
            _lastPosition = transform.position;
            _stuckTimer = 0f;
        }
        
        Vector3 GetNewWanderDirection() {
            if (wanderDirection == WanderDirection.RandomInCircle) {
                return new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
            }
            if (wanderDirection == WanderDirection.RandomCartesian) {
                return CartesianDirections[UnityEngine.Random.Range(0, CartesianDirections.Length)];
            }
            return Vector3.zero;
        }
        
        private void SetNewDestination()
        {
            if (wanderType == WanderType.NavMesh && TryGetRandomWalkablePosition(transform.position, wanderRadius, out Vector3 newTarget))
            {
                _targetPosition = newTarget;
            }
            else if (wanderType == WanderType.DirectionWalk)
            {
                // For DirectionWalk, we don't need a navmesh target - just set the direction
                _targetPosition = transform.position;
            }
            else
            {
                // Optional fallback if all attempts fail
                _targetPosition = transform.position;
                _path.ClearCorners();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(_targetPosition, 0.5f);
        }

        protected override void BumpedIntoSomething(Collider collider)
        {
            ChangeDirection();
        }

        protected override void ApproachedSomething(Collider collider)
        {
            ChangeDirection();
        }

        private void ChangeDirection() {
            // Get current direction and change it by at least 90 degrees
            _direction = GetBiasedDirection(_direction);
            _targetDirection = _direction;
            _stuckTimer = 0f; // Reset stuck timer when changing direction
        }

        private Vector3 GetBiasedDirection(Vector3 currentDirection) {
            // If we're not moving, return a random direction
            if (currentDirection.sqrMagnitude < 0.1f) {
                return CartesianDirections[UnityEngine.Random.Range(0, CartesianDirections.Length)];
            }
            
            // Create a list of directions that are at least 90 degrees different
            var validDirections = new System.Collections.Generic.List<Vector3>();
            
            // Check each possible direction
            foreach (var dir in CartesianDirections) {
                // Calculate the angle between current and new direction
                float angle = Vector3.SignedAngle(currentDirection, dir, Vector3.up);
                
                // Ensure it's at least 90 degrees (but not 180 exactly)
                if (Mathf.Abs(angle) >= 90f && Mathf.Abs(angle) < 180f) {
                    validDirections.Add(dir);
                }
            }
            
            // If we have valid directions, choose one randomly
            if (validDirections.Count > 0) {
                return validDirections[UnityEngine.Random.Range(0, validDirections.Count)];
            }
            
            // Fallback to random direction if all else fails
            return CartesianDirections[UnityEngine.Random.Range(0, CartesianDirections.Length)];
        }

        private void Update()
        {
            if (_isDead) return;
            
            switch (_wanderState)
            {
                case WanderState.Wandering:
                    _timer -= Time.deltaTime;
                    
                    // Check if NPC is stuck (not making progress)
                    CheckIfStuck();

                    if (_timer <= 0f)
                    {
                        if (wanderType == WanderType.DirectionWalk)
                        {
                            ChangeDirection();
                            _timer = UnityEngine.Random.Range(wanderTimeMin, wanderTimeMax);
                        }
                        else
                        {
                            SetNewDestination();
                            _timer = wanderInterval;
                        }
                    }

                    if (wanderType == WanderType.NavMesh) 
                    {
                        if (_path != null && _path.corners.Length > 1) 
                        {
                            Vector3 direction = (_path.corners[1] - transform.position).normalized;
                            MoveInDirection(direction);
                        }
                    }
                    else if (wanderType == WanderType.DirectionWalk)
                    {
                        // Direct movement in current direction
                        MoveInDirection(_direction);
                    }
                    break;

                case WanderState.Idling:
                    _timer -= Time.deltaTime;

                    if (_timer <= 0f)
                    {
                        StartWandering();
                    }
                    break;
            }
        }

        private void CheckIfStuck()
        {
            // Check if we've been in the same position for too long
            Vector3 currentPosition = transform.position;
            float distanceMoved = Vector3.Distance(_lastPosition, currentPosition);
            
            // If we haven't moved enough in the last STUCK_TIME seconds
            if (distanceMoved < stuckThreshold)
            {
                _stuckTimer += Time.deltaTime;
                
                // If we haven't moved anywhere for STUCK_TIME seconds, change direction
                if (_stuckTimer >= stuckTime)
                {
                    ChangeDirection();
                    _stuckTimer = 0f;
                    _timer = UnityEngine.Random.Range(wanderTimeMin, wanderTimeMax);
                }
            }
            else
            {
                // We're making progress, reset the stuck timer
                _stuckTimer = 0f;
            }
            
            // Update last position for next frame
            _lastPosition = currentPosition;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _wanderState = WanderState.Idling;
            _timer = 0f;
            _direction = Vector3.zero;
            _targetDirection = Vector3.zero;
            MoveInDirection(Vector3.zero);
        }
    }
}
