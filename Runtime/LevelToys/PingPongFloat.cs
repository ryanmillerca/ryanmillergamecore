namespace RyanMillerGameCore.LevelToys
{
    using System;
    using Utilities;
    using UnityEngine;
    
    public class PingPongFloat : MonoBehaviour, IFloatValueProvider
    {
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;
        [SerializeField] private float moveTime = 1f;
        [SerializeField] private float holdTime = 3f;

        public float CurrentValue => pingPongValue;
        public event Action<float> ValueChanged;

        private float pingPongValue;

        private enum State { Rising, HoldingMax, Falling, HoldingMin }
        private State _state = State.Rising;

        private float _timer = 0f;

        private void Update()
        {
            _timer += Time.deltaTime;

            switch (_state)
            {
                case State.Rising:
                    if (_timer < moveTime)
                    {
                        float t = _timer / moveTime;
                        pingPongValue = Mathf.Lerp(minValue, maxValue, t);
                        RaiseChange();
                    }
                    else
                    {
                        pingPongValue = maxValue;
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
                        pingPongValue = Mathf.Lerp(maxValue, minValue, t);
                        RaiseChange();
                    }
                    else
                    {
                        pingPongValue = minValue;
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
            ValueChanged?.Invoke(pingPongValue);
        }
    }
}