using UnityEngine.Serialization;
namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;
    using Animation;
    using Interactions;

    /// <summary>
    /// Grants CharacterSMBs access to the character's components.
    /// </summary>
    public class CharacterReferences : MonoBehaviour
    {
        [FormerlySerializedAs("movement")]
        [Header("References")]
        public CharacterMovement _movement;
        [FormerlySerializedAs("mainCollider")]
        public Collider _mainCollider;
        [FormerlySerializedAs("characterInput")]
        public CharacterInput _characterInput;
        [FormerlySerializedAs("characterAnimation")]
        public CharacterAnimation _characterAnimation;
        [FormerlySerializedAs("character")]
        public Character _character;
        [FormerlySerializedAs("characterBrain")]
        public CharacterBrain _characterBrain;
        [FormerlySerializedAs("playerCharacter")]
        public PlayerCharacter _playerCharacter;
        [FormerlySerializedAs("attackColliderSensor")]
        public ColliderSensor _attackColliderSensor;
        [FormerlySerializedAs("interactColliderSensor")]
        public InteractiveObjectColliderSensor _interactColliderSensor;
        [FormerlySerializedAs("aggroColliderSensor")]
        public ColliderSensor _aggroColliderSensor;
        [FormerlySerializedAs("rb")]
        public Rigidbody _rb;
        [FormerlySerializedAs("damageDealer")]
        public DamageDealer _damageDealer;
        [FormerlySerializedAs("animator")]
        public Animator _animator;
        [FormerlySerializedAs("renderers")]
        public Renderer[] _renderers;
        [FormerlySerializedAs("characterPathfind")]
        public CharacterPathfind _characterPathfind;
        
        [Header("Animation Parameters")]
        public string paramTriggerHurt = "hurt";
        public string paramBoolDead = "isDead";
        public string paramBoolAggro = "HasAggro";
        public string paramSpeedHorizontal = "speed_horizontal";
        public string paramTriggerAttack = "attack";
        public string paramTriggerInteract = "interact";

        [Header("Stats")] 
        public float attackDashForce = 500;
        
        void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
            _animator.SetComponentReference(this);
        }

        void OnDestroy()
        {
            if (_animator != null)
            {
                _animator.ClearComponentReference();
            }
        }
    }
}