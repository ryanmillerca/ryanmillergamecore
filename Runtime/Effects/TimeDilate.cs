namespace RyanMillerGameCore.Effects
{
    using UnityEngine;
    using System.Collections;

    public class TimeDilate : MonoBehaviour
    {
        [SerializeField] private float dilateDuration = 0.5f;
        [SerializeField] private AnimationCurve dilateCurve;

        private void OnEnable()
        {
            StartCoroutine(AnimateDilation());
        }

        private IEnumerator AnimateDilation()
        {
            for (float i = 0; i <= dilateDuration; i += Time.unscaledDeltaTime)
            {
                float t = i / dilateDuration;
                Time.timeScale = Mathf.Lerp(0, 1, dilateCurve.Evaluate(t));
                yield return new WaitForEndOfFrame();
            }
            Time.timeScale = 1;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            Time.timeScale = 1;
        }
    }
}