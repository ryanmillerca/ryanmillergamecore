namespace RyanMillerGameCore.Animation
{
    using System;
    using Character;
    using UnityEngine;

    public class AnimateFromMotor : MonoBehaviour
    {
        [SerializeField] private Animator characterAnimator;
        private CharacterAnimation _characterAnimation;
        [SerializeField] private string animParamSpeedHorizontal = "speed_horizontal";
        [NonSerialized] private int animParamSpeedHorizontalHash;
        [NonSerialized] private CharacterMovement characterMovement;

        private void OnEnable()
        {
            characterMovement = GetComponentInParent<CharacterMovement>();
            _characterAnimation = GetComponentInParent<CharacterAnimation>();
            if (characterAnimator == null)
            {
                Debug.LogError("Animator is null.", gameObject);
                return;
            }

            animParamSpeedHorizontalHash = Animator.StringToHash(animParamSpeedHorizontal);
            characterMovement.OnVelocityApplied += OnVelocityApplied;
        }

        private void OnDisable()
        {
            characterMovement.OnVelocityApplied -= OnVelocityApplied;
        }

        void OnVelocityApplied(float velocity)
        {
            characterAnimator.SetFloat(animParamSpeedHorizontalHash, velocity);
        }
    }
}