namespace RyanMillerGameCore.LevelToys
{
    using System;
    using UnityEngine;
    using Utilities;
    
    public class PingPongSpringFloat : MonoBehaviour, IFloatValueProvider
    {
        [Header("Value Range")]
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;

        [Header("Timing")]
        [SerializeField] private float moveTime = 1f;
        [SerializeField] private float holdTime = 2f;

        [Header("Motion Curve")]
        [Tooltip("Curve should go from 0 to 1 in time, with antic/overshoot in value.")]
        [SerializeField] private AnimationCurve overshootCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public float CurrentValue => _value;
        public event Action<float> ValueChanged;

        private enum State { Rising, HoldingMax, Falling, HoldingMin }
        private State _state = State.Rising;

        private float _timer = 0f;
        private float _value;

        private void Update()
        {
            _timer += Time.deltaTime;

            switch (_state)
            {
                case State.Rising:
                    if (_timer < moveTime)
                    {
                        float t = _timer / moveTime;
                        float curveT = overshootCurve.Evaluate(t);
                        _value = Mathf.LerpUnclamped(minValue, maxValue, curveT);
                        RaiseChange();
                    }
                    else
                    {
                        _value = maxValue;
                        RaiseChange();
                        TransitionTo(State.HoldingMax);
                    }
                    break;

                case State.HoldingMax:
                    if (_timer >= holdTime)
                        TransitionTo(State.Falling);
                    break;

                case State.Falling:
                    if (_timer < moveTime)
                    {
                        float t = _timer / moveTime;
                        float curveT = overshootCurve.Evaluate(t);
                        _value = Mathf.LerpUnclamped(maxValue, minValue, curveT);
                        RaiseChange();
                    }
                    else
                    {
                        _value = minValue;
                        RaiseChange();
                        TransitionTo(State.HoldingMin);
                    }
                    break;

                case State.HoldingMin:
                    if (_timer >= holdTime)
                        TransitionTo(State.Rising);
                    break;
            }
        }

        private void TransitionTo(State next)
        {
            _state = next;
            _timer = 0f;
        }

        private void RaiseChange()
        {
            ValueChanged?.Invoke(_value);
        }
    }
}