namespace RyanMillerGameCore.LevelToys
{
    using UnityEngine;

    [RequireComponent(typeof(SpringJoint))]
    [RequireComponent(typeof(LineRenderer))]
    public class VineRenderer : MonoBehaviour
    {
        [Header("Wiggly Line")]
        [SerializeField] private int segments = 20;
        [SerializeField] private float waveAmplitude = 0.1f;
        [SerializeField] private float waveFrequency = 5f;
        [SerializeField] private Vector3 posOffset;

        [Header("Snap Animation")]
        [SerializeField] private float settleDuration = 0.5f;
        [SerializeField] private AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private SpringJoint _springJoint;
        private LineRenderer _lineRenderer;
        private Vector3[] _previousPositions;
        private float _snapTimer = -1f;
        private bool _animatingSnap = false;
        private Vector3 _collapseTarget;
        private bool _wasConnectedLastFrame = false;

        private void Awake()
        {
            _springJoint = GetComponent<SpringJoint>();
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = segments + 1;

            _previousPositions = new Vector3[segments + 1];
        }

        private void DrawWigglyLine(Vector3 start, Vector3 end)
        {
            Vector3 direction = end - start;
            Vector3 up = Vector3.Cross(direction.normalized, Vector3.forward).normalized;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 point = Vector3.Lerp(start, end, t);
                float offset = Mathf.Sin(t * waveFrequency * Mathf.PI * 2f) * waveAmplitude;
                point += up * offset;
                _lineRenderer.SetPosition(i, point + posOffset);
            }
        }

        private void Update()
        {
            bool isConnected = _springJoint.connectedBody != null;

            if (isConnected)
            {
                _wasConnectedLastFrame = true;
                _animatingSnap = false;

                Vector3 startPoint = transform.TransformPoint(_springJoint.anchor);
                Vector3 endPoint = _springJoint.connectedBody.transform.TransformPoint(_springJoint.connectedAnchor);

                DrawWigglyLine(startPoint, endPoint);

                for (int i = 0; i <= segments; i++)
                    _previousPositions[i] = _lineRenderer.GetPosition(i);

                _lineRenderer.enabled = true;
            }
            else
            {
                // Detect disconnect just once
                if (_wasConnectedLastFrame)
                {
                    _snapTimer = 0f;
                    _animatingSnap = true;
                    _collapseTarget = transform.TransformPoint(_springJoint.anchor);
                    _lineRenderer.enabled = true;
                    _wasConnectedLastFrame = false;
                }

                if (_animatingSnap)
                {
                    AnimateCollapse();
                }
            }
        }

        private void AnimateCollapse()
        {
            _snapTimer += Time.deltaTime;
            float t = _snapTimer / settleDuration;

            if (t >= 1f)
            {
                _lineRenderer.enabled = false;
                _animatingSnap = false;
                return;
            }

            float curveValue = snapCurve.Evaluate(t);
            for (int i = 0; i <= segments; i++)
            {
                Vector3 from = _previousPositions[i];
                Vector3 to = _collapseTarget;
                Vector3 snapped = Vector3.LerpUnclamped(from, to, curveValue);
                _lineRenderer.SetPosition(i, snapped);
            }
        }
    }
}