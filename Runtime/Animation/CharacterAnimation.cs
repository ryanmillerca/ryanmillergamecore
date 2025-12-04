namespace RyanMillerGameCore.Animation
{
    using UnityEngine;
    using System.Collections;
    using Character;

    public class CharacterAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        
        [Header("Params")]
        [SerializeField] private int cosmeticAnimLayer = 1;
        [SerializeField] private float cosmeticAnimDuration = 5;
        [SerializeField] private float blendTime = 0.25f;
        
        [Header("Direction/Facing")]
        [Tooltip("Sends rotation/direction to animator as 0-3 value (0=N, 1=E, 2=S, 3=W")]
        [SerializeField] private bool sendDirectionToAnimator = true;
        [SerializeField] private string directionParameterName = "Direction";
        [SerializeField] private int directionIndexOffset = 0;
        [SerializeField] private float paramMultiplier = 0.1f;
        [SerializeField] private float minThreshold = 0.001f;
        private Coroutine _weightChanges;
        private CharacterMovement _characterMovement;
        
        private void OnEnable()
        {
            if (animator == null) {
                animator = GetComponentInChildren<Animator>();
            }
            if (sendDirectionToAnimator) {
                _characterMovement = GetComponent<CharacterMovement>();
                _characterMovement.OnMoveInDirection += OnMoveInDirection;
            }
        }

        private void OnDisable() {
            if (sendDirectionToAnimator) {
                _characterMovement.OnMoveInDirection -= OnMoveInDirection;
            }
        }

        private void OnMoveInDirection(Vector3 inputDirection)
        {
            if (inputDirection.sqrMagnitude > minThreshold)
            {
                float angle = Mathf.Atan2(inputDirection.z, inputDirection.x);
                float angleDegrees = -angle * Mathf.Rad2Deg;
                float normalizedAngle = Mathf.Repeat(angleDegrees, 360f);
                float rotationIndex = (Mathf.FloorToInt(normalizedAngle / 90f) + 1) % 4;
                animator.SetFloat(directionParameterName, rotationIndex * paramMultiplier);
            }
        }

        public void PlayAnimation(string clipName, float customDuration = -1) {
            if (string.IsNullOrEmpty(clipName)) {
                return;
            }
            animator.Play(clipName, cosmeticAnimLayer, 0);

            float animDuration = customDuration;
            if (animDuration <= 0)
            {
                animDuration = GetClipDuration(clipName);
            }

            if (_weightChanges != null)
            {
                StopCoroutine(_weightChanges);
            }
            _weightChanges = StartCoroutine(SetLayerWeight(animDuration));
        }

        public void PlayAnimation(AnimationClip clip, float customDuration = -1)
        {
            if (!clip)
            {
                return;
            }
            PlayAnimation(clip.name, customDuration);
        }

        private IEnumerator SetLayerWeight(float duration)
        {
            // blend in weight
            for (float i = 0; i <= blendTime; i += Time.unscaledDeltaTime)
            {
                animator.SetLayerWeight(cosmeticAnimLayer,i/blendTime);
                yield return new WaitForEndOfFrame();
            }
            animator.SetLayerWeight(cosmeticAnimLayer,1);
            
            // wait for duration (minus blend time)
            duration -= blendTime * 2;
            yield return new WaitForSecondsRealtime(duration);
            
            // blend out weight
            for (float i = 0; i <= blendTime; i += Time.unscaledDeltaTime)
            {
                animator.SetLayerWeight(cosmeticAnimLayer,1-i/blendTime);
                yield return new WaitForEndOfFrame();
            }
            animator.SetLayerWeight(cosmeticAnimLayer,0);
        }

        private float GetClipDuration(string clipName)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.Equals(clipName))
                {
                    return clip.length;
                }
            }
            return 1;
        }
    }
}