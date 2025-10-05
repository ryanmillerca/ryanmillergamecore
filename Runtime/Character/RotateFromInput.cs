namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    
    public class RotateFromInput : MonoBehaviour
    {
        [SerializeField] private float rotationOffset;
        [SerializeField] private float minThreshold = 0.001f;
        [SerializeField] private float lerpSpeed = 6;
        
        private CharacterBrain characterBrain; 
        private Quaternion targetRotation;
        
        private void OnEnable()
        {
            if (characterBrain == null)
            {
                characterBrain = transform.root.GetComponent<CharacterBrain>();
            }
            characterBrain.OnMoveCameraSpace += MoveCameraSpace;
        }
        
        private void OnDisable()
        {
            characterBrain.OnMoveCameraSpace -= MoveCameraSpace;
        }

        private void MoveCameraSpace(Vector3 inputDirection)
        {
            if (inputDirection.sqrMagnitude > minThreshold)
            {
                float angle = Mathf.Atan2(inputDirection.z, inputDirection.x);
                float angleDegrees = -angle * Mathf.Rad2Deg;
                targetRotation = Quaternion.Euler(0, angleDegrees + rotationOffset, 0);
                if (lerpSpeed <= 0)
                {
                    transform.localRotation = targetRotation;
                }
            }
        }

        private void Update()
        {
            if (lerpSpeed > 0)
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * lerpSpeed);
            }
        }
    }
}