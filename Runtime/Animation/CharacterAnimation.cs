namespace RyanMillerGameCore.Animation
{
    using UnityEngine;
    using System.Collections;

    public class CharacterAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip testAnim;
        [SerializeField] private int cosmeticAnimLayer = 1;
        [SerializeField] private float cosmeticAnimDuration = 5;
        [SerializeField] private float blendTime = 0.25f;
        private Coroutine _weightChanges;
        
        public void PlayAnimation(string clipName, float customDuration = -1)
        {
            if (string.IsNullOrEmpty(clipName))
            {
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
        
        [ContextMenu("Test Animation")]
        private void TestAnim()
        {
            PlayAnimation(testAnim, cosmeticAnimDuration);
        }
    }
}