namespace RyanMillerGameCore.Character
{
    using System;
    using UnityEngine;

    public class WanderingBrain : CharacterBrain
    {
        [SerializeField] private float wanderTimeMin = 2f;
        [SerializeField] private float wanderTimeMax = 5f;
        [SerializeField] private float wanderInterval = 3f;
        [SerializeField] private float wanderRadius = 5f;

        private enum WanderState { Wandering, Idling }
        private WanderState _wanderState;

        [NonSerialized] private float _timer;
        [NonSerialized] private Vector3 _direction;
        [NonSerialized] private Vector3 _targetDirection;
        
        protected override void Awake()
        {
            base.Awake();
            StartWandering();
        }

        #pragma warning disable CS0252

        private void StartWandering()
        {
            _timer = UnityEngine.Random.Range(wanderTimeMin, wanderTimeMax);
            _targetDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
            _direction = _targetDirection; // Ensure smooth start
            _wanderState = WanderState.Wandering;
        }
        
        private void SetNewDestination()
        {
            if (TryGetRandomWalkablePosition(transform.position, wanderRadius, out Vector3 newTarget))
            {
                _targetPosition = newTarget;
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

        private void ChangeDirection()
        {
            // Use quaternion for more reliable rotation
            Quaternion turn = Quaternion.Euler(0, UnityEngine.Random.Range(-90f, 90f), 0);
            _direction = (turn * _direction).normalized;
            _targetDirection = _direction;
        }

        private void Update()
        {
            if (_isDead) return;
            
            switch (_wanderState)
            {
                case WanderState.Wandering:
                    _timer -= Time.deltaTime;

                    if (_timer <= 0f)
                    {
                        SetNewDestination();
                        _timer = wanderInterval;
                    }

                    if (_path != null && _path.corners.Length > 1)
                    {
                        Vector3 direction = (_path.corners[1] - transform.position).normalized;
                        MoveInDirection(direction);
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
