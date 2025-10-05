namespace RyanMillerGameCore.Animation
{
    using UnityEngine;

    public class AnimatedState : MonoBehaviour
    {
        [SerializeField] private CharacterAnimation playableAnimationController;
        [SerializeField] private AnimationClip animationClip;
        [SerializeField] protected AnimBehaviour animationBehaviour = AnimBehaviour.LeaveAsIs;

        protected enum AnimBehaviour
        {
            LeaveAsIs,
            UseAnimator,
            UsePlayableGraph
        }

        private CharacterAnimation PlayableAnimationController
        {
            get
            {
                if (playableAnimationController == null)
                {
                    playableAnimationController = GetComponentInParent<CharacterAnimation>();
                }

                return playableAnimationController;
            }
        }

        public void PlayAnimation()
        {
            if (animationClip == null)
            {
                return;
            }

            if (animationBehaviour == AnimBehaviour.UsePlayableGraph)
            {
                PlayableAnimationController.PlayAnimation(animationClip.name);
            }
        }

        public void StopAnimation()
        {
            if (animationBehaviour == AnimBehaviour.UsePlayableGraph)
            {
            }
        }
    }
}