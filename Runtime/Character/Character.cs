namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    using System;
    using UnityEngine.Events;
    using SaveSystem;
    using Camera;
    using Interactions;
    using Utilities;
    using SMB;

    /// <summary>
    /// Represents a general character in the game, managing health, damage, player status, and related events.
    /// </summary>
    [RequireComponent(typeof(AutoRegisterID))]
    public class Character : MonoBehaviour, ICharacter, ITakesDamage, IHasID
    {
        [Header("Parameters")]
        [SerializeField] private CharacterID identifier;
        [SerializeField] private float maxHealth = 10;
        [SerializeField] private float currentHealth = 10;
        [SerializeField] private bool isPlayer;
        [SerializeField] private bool spawnAtCheckpoint = false;
        [SerializeField] private float damageCooldown = 1f;
        
        [Foldout("Unity Events"), SerializeField] private UnityEvent<float> receivedDamage;
        [Foldout("Unity Events"), SerializeField] private UnityEvent<float> receivedHeal;
        [Foldout("Unity Events"), SerializeField] private UnityEvent knockedBack;
        [Foldout("Unity Events"), SerializeField] private UnityEvent died;

        public CharacterReferenceProvider referenceProvider
        {
            get
            {
                if (m_characterReferenceProvider == null)
                {
                    m_characterReferenceProvider = GetComponentInChildren<CharacterReferenceProvider>();
                }
                return m_characterReferenceProvider;
            }
        }
        
        public event Action<float> OnReceiveDamage;
        public event Action<float> OnReceiveHeal;
        public event Action<Character> OnDied;
        public event Action<Vector3> OnKnockedBack;
        public event Action Died;
        public event Action Spawned;

        private CharacterReferenceProvider m_characterReferenceProvider;
        private CharacterMovement _characterMovement;
        private CharacterBrain _characterBrain;
        private static bool isQuitting = false;
        private float lastDamageTime = -Mathf.Infinity;
        
        public CharacterMovement CharacterMovement
        {
            get
            {
                if (!_characterMovement)
                {
                    _characterMovement = GetComponent<CharacterMovement>();
                }
                return _characterMovement;
            }
        }

        public CharacterBrain Brain
        {
            get
            {
                if (_characterBrain == null)
                {
                    _characterBrain = GetComponent<CharacterBrain>();
                }
                return _characterBrain;
            }
        }

        public float PercentHealth
        {
            get { return currentHealth / maxHealth; }
        }
        
        public bool IsPlayer()
        {
            return isPlayer;
        }
        
        public CharacterID ID()
        {
            return identifier;
        }

        public bool Respawn()
        {
            if (currentHealth > 0)
            {
                return false;
            }
            GoToCheckpoint();
            Reset();
            Spawned?.Invoke();
            
            return true;
        }

        public void GoToCheckpoint(CheckpointData checkpointData = null) {
            if (!DataManager.Instance) {
                Debug.LogWarning("GoToCheckpoint failed: No DataManager in scene");
                return;
            }
            checkpointData ??= DataManager.Instance.TryGetCheckpoint();
            if (checkpointData == null) {
                //Debug.LogWarning("GoToCheckpoint failed: No checkpoint data found");
                return;
            }
            //Debug.Log("GoToCheckpoint: Found a checkpoint at " + checkpointData.position + ", teleporting player");
            
            CharacterMovement.Teleport(checkpointData.position, checkpointData.rotation);
            
            if (!CameraController.Instance) {
                Debug.LogWarning("GoToCheckpoint issue: No Camera Controller in scene");
                return;
            }
            CameraController.Instance.TargetYRotation = checkpointData.cameraRotation;
            CameraController.Instance.transform.position = checkpointData.position;
        }
        

        public void Reset()
        {
            currentHealth = maxHealth;
        }

        public void Interact(IInteractive interactive)
        {
            interactive.Interact(this);
        }

        public bool CanReceiveDamage()
        {
            // cannot, in damage cooldown period / iframes
            if (Time.time < lastDamageTime + damageCooldown) {
                return false;
            }
            // cannot, if dead
            if (currentHealth <= 0) {
                return false;
            }
            // well we must be alive, then
            lastDamageTime = Time.time;
            return true;
        }

        public bool CanReceiveHealing()
        {
            return currentHealth < maxHealth;
        }

        public bool ReceiveDamage(float damageAmount, Component attacker = null)
        {
            if (damageAmount == 0 || currentHealth <= 0)
            {
                return false;
            }

            currentHealth -= damageAmount;
            OnReceiveDamage?.Invoke(damageAmount);
            receivedDamage?.Invoke(damageAmount);
            if (currentHealth <= 0)
            {
                Brain.GoToDead();
                OnDied?.Invoke(this);
                Died?.Invoke();
                died?.Invoke();
            }
            else
            {
                Brain.GoToHurt();
            }

            return true;
        }
        
        public bool ReceiveHeal(float healAmount, bool overHeal = false)
        {
            if (healAmount == 0)
            {
                return false;
            }

            currentHealth += healAmount;
            if (overHeal == false)
            {
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            }

            OnReceiveHeal?.Invoke(healAmount);
            receivedHeal?.Invoke(healAmount);
            return true;
        }

        public bool ReceiveKnockback(Vector3 direction)
        {
            if (direction.magnitude <= 0.01f)
            {
                return false;
            }

            OnKnockedBack?.Invoke(direction);
            knockedBack?.Invoke();
            return true;
        }

        public void GetKnockback(Vector3 direction)
        {
            ReceiveKnockback(direction); 
        }

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            if (transform.parent != null)
            {
                //transform.SetParent(null);
            }
            
            RegisterToCharacterManager();

            if (spawnAtCheckpoint)
            {
                Respawn();
            }
            else
            {
                Spawned?.Invoke();
            }
        }
        
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            isQuitting = false;
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        private void OnDestroy()
        {
            if (!isQuitting && Application.isPlaying)
            {
                if (CharacterManager.Instance)
                {
                    CharacterManager.Instance.RemoveCharacter(this);
                }
            }
        }

        private void RegisterToCharacterManager()
        {
            if (CharacterManager.Instance == null)
            {
                return;
            }
            CharacterManager.Instance.RegisterCharacter(this);
        }

        public ID GetID()
        {
            return identifier;
        }
    }
}