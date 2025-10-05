namespace RyanMillerGameCore.LevelToys
{
    using UnityEngine;
    using UnityEngine.Playables;
    using Utilities;

    [RequireComponent(typeof(PlayableDirector))]
    public class FloatDrivenTimeline : MonoBehaviour
    {
        [Tooltip("Source that provides float values (must implement IFloatValueProvider).")] [SerializeField]
        private MonoBehaviour inputSource; // Must implement IFloatValueProvider

        [SerializeField] private float valueMultiplier = 0.001f;

        private IFloatValueProvider _floatProvider;
        private PlayableDirector _director;

        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();

            if (inputSource == null)
            {
                Debug.LogError("FloatDrivenTimeline: Input source is not assigned.");
                enabled = false;
                return;
            }

            _floatProvider = inputSource as IFloatValueProvider;

            if (_floatProvider == null)
            {
                Debug.LogError("FloatDrivenTimeline: Input source does not implement IFloatValueProvider.");
                enabled = false;
                return;
            }

            _director.playableGraph.GetRootPlayable(0).SetSpeed(0);
            _floatProvider.ValueChanged += OnFloatValueChanged;
        }

        private void OnDestroy()
        {
            if (_floatProvider != null)
            {
                _floatProvider.ValueChanged -= OnFloatValueChanged;
            }
        }

        private void OnFloatValueChanged(float value)
        {
            if (_director.playableAsset == null)
            {
                return;
            }

            double duration = _director.duration;
            double time = (value * valueMultiplier) * duration;
            time = Mathf.Clamp((float)time, 0, (float)duration);
            _director.time = time;
            _director.Evaluate();
        }
    }
}