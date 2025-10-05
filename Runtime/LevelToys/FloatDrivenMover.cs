namespace RyanMillerGameCore.LevelToys
{
    using UnityEngine;
    using Utilities;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    public class FloatDrivenMover : MonoBehaviour
    {
        [Tooltip("Source that provides float values (must implement IFloatValueProvider).")]
        [SerializeField] private MonoBehaviour inputSource; // Must implement IFloatValueProvider

        [Tooltip("Final position reached when input value reaches Target Value.")]
        [SerializeField] private Vector3 endPosition;

        [Tooltip("Input value required to reach full end position.")]
        [SerializeField] private float targetValue = 1000f;

        private IFloatValueProvider _provider;
        private Vector3 _startPosition;

        #if UNITY_EDITOR
        [ContextMenu("Set Start Position")]
        private void SetStartPosition()
        {
            Undo.RecordObject(this, "Set Start Position");
            _startPosition = transform.localPosition;
        }
        
        [ContextMenu("Set End Position")]
        private void SetEndPosition()
        {
            Undo.RecordObject(this, "Set End Position");
            endPosition = transform.localPosition;
        }

        [ContextMenu("Go to Start Position")]
        private void GoToStartPosition()
        {
            Undo.RecordObject(this, "Go To Start Position");
            transform.localPosition = _startPosition;
        }
        
        #endif
        
        private void Awake()
        {
            _startPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            if (inputSource is IFloatValueProvider provider)
            {
                _provider = provider;
                _provider.ValueChanged += HandleInputChanged;
            }
            else if (inputSource != null)
            {
                Debug.LogError($"{inputSource.name} does not implement IFloatValueProvider", this);
            }
        }

        private void OnDisable()
        {
            if (_provider != null)
            {
                _provider.ValueChanged -= HandleInputChanged;
                _provider = null;
            }
        }

        private void HandleInputChanged(float value)
        {
            float t = value / targetValue;
            transform.localPosition = Vector3.LerpUnclamped(_startPosition, endPosition, t);
        }
    }
}