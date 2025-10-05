namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    
    /// <summary>
    /// Manages player-specific behavior and references, including singleton access and positional data.
    /// </summary>
    public class PlayerCharacter : MonoBehaviour
    {
        public static PlayerCharacter Instance { get; private set; } 

        public Character Character
        {
            get
            {
                if (_character == null)
                {
                    _character = GetComponent<Character>();
                }
                return _character;
            }
        }
        
        [SerializeField] private float frontOffset = 0.7f;
        [SerializeField] private Transform rotatingTransform;

        private Character _character;
        
        public Vector3 GetFrontPosition()
        {
            return rotatingTransform.position + (rotatingTransform.forward * frontOffset);
        }
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(GetFrontPosition(), 0.25f);
        }
    }
}