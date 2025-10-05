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
        [Header("References")]
        public CharacterMovement movement;
        public Collider mainCollider;
        public CharacterInput characterInput;
        public CharacterAnimation characterAnimation;
        public Character character;
        public CharacterBrain characterBrain;
        public PlayerCharacter playerCharacter;
        public ColliderSensor attackColliderSensor;
        public InteractiveObjectColliderSensor interactColliderSensor;
        public ColliderSensor aggroColliderSensor;
        public Rigidbody rb;
        public DamageDealer damageDealer;
        public Animator animator;
        public Renderer[] renderers;
        public CharacterPathfind characterPathfind;
        
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
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            animator.SetComponentReference(this);
        }

        void OnDestroy()
        {
            if (animator != null)
            {
                animator.ClearComponentReference();
            }
        }
    }
}