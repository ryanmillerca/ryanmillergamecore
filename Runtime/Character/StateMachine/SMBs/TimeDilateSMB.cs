namespace RyanMillerGameCore.Character.SMB
{
    using System.Collections;
    using Utilities;
    using UnityEngine;
    
    public class TimeDilateSMB : CharacterSMB
    {
        [SerializeField, Range(0,1)] private float dilateDuration = 1;
        [SerializeField] private AnimationCurve dilateCurve;

        private float _dilateDuration;
        private Coroutine _dilationCoroutine;

        protected override void OnCharacterStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnCharacterStateEnter(animator, stateInfo, layerIndex);
            _dilateDuration = stateInfo.length * dilateDuration;
            _dilationCoroutine = CoroutineRunner.Instance.StartCoroutine(AnimateDilation());
        }

        private IEnumerator AnimateDilation()
        {
            for (float i = 0; i <= _dilateDuration; i += Time.unscaledDeltaTime)
            {
                float t = i / _dilateDuration;
                Time.timeScale = Mathf.Lerp(0, 1, dilateCurve.Evaluate(t));
                yield return new WaitForEndOfFrame();
            }
            Time.timeScale = 1;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            CoroutineRunner.Instance.StopCoroutine(_dilationCoroutine);
            Time.timeScale = 1;
        }
    }
}