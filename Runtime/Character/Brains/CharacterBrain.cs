namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    using Utilities;
    using System;
    using SMB;
    using System.Collections.Generic;
    using UnityEngine.AI;

    public class CharacterBrain : MonoBehaviour
    {
        
        #region Public Methods

        public void Attack()
        { 
            OnAttackAction?.Invoke();
        }

        public void Interact()
        {
            OnInteractAction?.Invoke();
        }

        public void MoveInDirection(Vector3 direction)
        {
            OnMoveCameraSpace?.Invoke(direction);
        }

        public void Teleport(Vector3 position)
        {
            OnTeleport?.Invoke(position);
        }

        public void Aggro()
        {
            OnAggroAction?.Invoke();
        }

        public void GoToIdle()
        {
            MoveInDirection(Vector3.zero);
        }

        public void GoToHurt()
        {
            References.animator.SetTrigger(_hurtTriggerHash);
        }

        public void GoToDead()
        {
            References.animator.SetBool(_deadBoolStringHash, true);
            if (turnOffColliderOnDeath)
            {
                SetCollision(false);
            }

            _isDead = true;
        }

        public void GoToMove()
        {
        }

        public void GoToRespawn()
        {
            if (turnOffColliderOnDeath)
            {
                SetCollision(true);
            }
            References.animator.SetBool(_deadBoolStringHash, false);
        }

        public void LookAt(Vector3 point)
        {
            _movable.LookAt(point);
        }

        public void SetInputEnabled(bool inputEnabled)
        {
            if (References.characterInput)
            {
                References.characterInput.SetInputEnabled(inputEnabled);
            }
        }

        public void SetMovementEnabled(bool movementEnabled)
        {
            if (References.characterInput)
            {
                References.characterInput.SetMovementEnabled(movementEnabled);
            }
        }

        public void SetAttackEnabled(bool attackEnabled)
        {
            if (References.characterInput)
            {
                References.characterInput.SetAttackEnabled(attackEnabled);
            }
        }

        public void AskToSetAttackEnabled(int askingID, bool attackEnabled)
        {
            // Remove from both sets to ensure a clean slate
            _attackEnableRequests.Remove(askingID);
            _attackDisableRequests.Remove(askingID);

            // Add to the relevant set
            if (attackEnabled)
            {
                _attackEnableRequests.Add(askingID);
            }
            else
            {
                _attackDisableRequests.Add(askingID);
            }

            bool shouldEnableAttack = _attackDisableRequests.Count == 0 && _attackEnableRequests.Count > 0;
            SetAttackEnabled(shouldEnableAttack);
        }
        
        public void SetInteractEnabled(bool interactEnabled)
        {
            if (References.characterInput)
            {
                References.characterInput.SetInteractEnabled(interactEnabled);
            }
        }

        public void SetAggroTarget(Transform newTarget, bool hasAggro)
        {
            _target = newTarget;

            if (_targetDamageTaker != null)
            {
                // unsubscribe from previous target's death event
                if (_targetDamageTaker != null)
                {
                    _targetDamageTaker.Died -= AggroTargetDied;
                }
            }

            if (newTarget != null)
            {
                // subscribe to new target's death event
                _targetDamageTaker = newTarget.GetComponentInParent(typeof(ITakesDamage)) as ITakesDamage;
                if (_targetDamageTaker != null)
                {
                    _targetDamageTaker.Died += AggroTargetDied;
                }
            }

            // change to aggro state
            References.animator.SetBool(_hasAggroHash, hasAggro);
        }

        protected virtual void AggroTargetDied()
        {
            SetAggroTarget(null, false);
        }

        #endregion


        #region Public Events

        public event Action<Vector3> OnMoveCameraSpace;
        public event Action<Vector3> OnTeleport;
        public event Action OnAttackAction;
        public event Action OnAggroAction;
        public event Action OnInteractAction;

        #endregion


        #region Public Properties

        public Transform Target => _target;

        public CharacterReferences References
        {
            get
            {
                if (_references == null)
                {
                    _references = GetComponentInChildren<CharacterReferences>();
                }
                return _references;
            }
        }

        #endregion


        #region Serialized Fields

        protected CharacterReferences characterReferences;

        [Tooltip("This is a 0-1 Dot Product for a frontal cone"), SerializeField, Range(0, 1)]
        private float bumpFrontAngle = 0.33f;

        [Tooltip("This is a 0-1 Dot Product for a frontal cone"), SerializeField, Range(0, 1)]
        private float seeFrontAngle = 0.33f;

        [SerializeField] private bool turnOffColliderOnDeath = true;
        
        #endregion


        #region Protected Fields

        protected bool _isDead;
        protected Transform _target;
        protected NavMeshPath _path;
        protected Vector3 _targetPosition;
        
        #endregion

        
        #region Private Fields

        private CharacterReferences _references;
        private IMovable _movable;
        private CollisionHandler _collisionHandler;
        private int _hurtTriggerHash;
        private int _deadBoolStringHash;
        private int _hasAggroHash;
        private ITakesDamage _targetDamageTaker;
        private readonly HashSet<int> _attackEnableRequests = new HashSet<int>();
        private readonly HashSet<int> _attackDisableRequests = new HashSet<int>();

        #endregion


        #region Protected Methods

        protected virtual void Awake()
        {
            _collisionHandler = GetComponent<CollisionHandler>();
            _movable = GetComponent<IMovable>();
            _hurtTriggerHash = Animator.StringToHash(References.paramTriggerHurt);
            _deadBoolStringHash = Animator.StringToHash(References.paramBoolDead);
            _hasAggroHash = Animator.StringToHash(References.paramBoolAggro);
            _path = new NavMeshPath();
        }

        protected virtual void OnEnable()
        {
            if (_collisionHandler != null)
            {
                _collisionHandler.CollisionEnter += OnCollision;
                _collisionHandler.TriggerEnter += OnTrigger;
            }

            References.character.Spawned += GoToRespawn;
            
            if (_movable != null)
            {
                OnMoveCameraSpace += _movable.Move;
                OnTeleport += _movable.Teleport;
            }
        }

        protected virtual void OnDisable()
        {
            if (_collisionHandler != null)
            {
                _collisionHandler.CollisionEnter -= OnCollision;
                _collisionHandler.TriggerEnter -= OnTrigger;
            }
            
            References.character.Spawned -= GoToRespawn;

            if (_movable != null)
            {
                OnMoveCameraSpace -= _movable.Move;
                OnTeleport -= _movable.Teleport;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_collisionHandler != null)
            {
                _collisionHandler.CollisionEnter -= OnCollision;
                _collisionHandler.TriggerEnter -= OnTrigger;
            }
        }
        
        protected virtual void BumpedIntoSomething(Collider collider)
        {
        }

        protected virtual void ApproachedSomething(Collider collider)
        {
        }
        
        protected bool TrySetPathToTarget(Vector3 destination)
        {
            return NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, _path)
                   && _path.status == NavMeshPathStatus.PathComplete;
        }
        
        protected bool TryGetRandomWalkablePosition(Vector3 center, float radius, out Vector3 result)
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
                randomDirection.y = 0;
                Vector3 candidate = center + randomDirection;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    if (NavMesh.CalculatePath(center, hit.position, NavMesh.AllAreas, _path) &&
                        _path.status == NavMeshPathStatus.PathComplete)
                    {
                        result = hit.position;
                        return true;
                    }
                }
            }

            result = center;
            return false;
        }

        #endregion

        
        #region Private Methods

        private void OnCollision(Collision collision)
        {
            if (_isDead) return;

            Vector3 collisionDirection = (collision.contacts[0].point - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, collisionDirection);
            if (dotProduct > bumpFrontAngle)
            {
                BumpedIntoSomething(collision.collider);
            }
        }

        private void OnTrigger(Collider collider)
        {
            if (_isDead) return;

            Vector3 collisionDirection = (collider.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, collisionDirection);
            if (dotProduct > seeFrontAngle)
            {
                ApproachedSomething(collider);
            }
        }

        private void SetCollision(bool collisionEnabled)
        {
            References.mainCollider.enabled = collisionEnabled;
            References.rb.isKinematic = !collisionEnabled;
        }

        #endregion
    }
}