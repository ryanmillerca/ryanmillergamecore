namespace RyanMillerGameCore.Dialog
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Positions the bubble UI above the current speaker and rebuilds layout when text changes.
    /// Does NOT touch alpha—DialogBubbleAnims is the single owner of fade/scale.
    /// </summary>
    public class DialogBubble : MonoBehaviour
    {
        [SerializeField] private DialogPlayer dialogPlayer;
        [SerializeField] private LayoutGroup layoutGroup;
        [SerializeField] private RectTransform dialogWindow;
        [SerializeField] private Vector2 windowOffset = new Vector2(0, 100);
        [SerializeField] private bool clampPosition = true;

        [Tooltip("Optional: used only to trigger layout rebuilds on text change.")] [SerializeField]
        private TextMeshProUGUI sourceText;

        private Camera _camera;
        private RectTransform _parentRect;
        private Transform _talkerWorldTransform;
        private Canvas _canvas;
        private bool _dialogActive;

        private Camera mainCamera
        {
            get
            {
                if (_camera == null) _camera = Camera.main;
                return _camera;
            }
        }

        private void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            _parentRect = GetComponent<RectTransform>();

            if (dialogPlayer != null)
            {
                dialogPlayer.DialogTextChanged += OnTextChanged;
                dialogPlayer.DialogOpened += OnDialogOpened;
                dialogPlayer.DialogClosed += OnDialogClosed;
                dialogPlayer.SpeakerChanged += OnSpeakerChanged;
            }

            // One-time layout sanity
            StartCoroutine(RefreshLayoutGroup());
        }

        private void OnDestroy()
        {
            if (!dialogPlayer) return;
            dialogPlayer.DialogTextChanged -= OnTextChanged;
            dialogPlayer.DialogOpened -= OnDialogOpened;
            dialogPlayer.DialogClosed -= OnDialogClosed;
            dialogPlayer.SpeakerChanged -= OnSpeakerChanged;
        }

        private void OnDialogOpened()
        {
            _dialogActive = true;
            StartCoroutine(RefreshLayoutGroup());
        }

        private void OnDialogClosed()
        {
            _dialogActive = false;
        }

        private void OnTextChanged()
        {
            // Rebuild layout on content change; leave visibility to the animator.
            StartCoroutine(RefreshLayoutGroup());
        }

        private void OnSpeakerChanged(Transform tr)
        {
            _talkerWorldTransform = tr;
        }

        private void Update()
        {
            if (_dialogActive)
            {
                PlaceBubbleAboveSpeaker();
            }
        }

        private IEnumerator RefreshLayoutGroup()
        {
            if (layoutGroup == null) yield break;
            layoutGroup.enabled = false;
            yield return new WaitForEndOfFrame();
            layoutGroup.enabled = true;
        }

        private void PlaceBubbleAboveSpeaker()
        {
            if (_talkerWorldTransform == null || _parentRect == null || dialogWindow == null) return;

            Vector2 parentSize = _parentRect.rect.size;
            Vector3 screenPos = mainCamera != null
                ? mainCamera.WorldToScreenPoint(_talkerWorldTransform.position)
                : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect,
                screenPos,
                _canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out Vector2 localPoint
            );

            Vector2 dialogPos = localPoint + windowOffset;

            if (clampPosition)
            {
                Vector2 dialogSize = dialogWindow.rect.size;
                Vector2 pivot = dialogWindow.pivot;

                float minX = -parentSize.x * 0.5f + dialogSize.x * pivot.x;
                float maxX = parentSize.x * 0.5f - dialogSize.x * (1f - pivot.x);
                float minY = -parentSize.y * 0.5f + dialogSize.y * pivot.y;
                float maxY = parentSize.y * 0.5f - dialogSize.y * (1f - pivot.y);

                dialogPos.x = Mathf.Clamp(dialogPos.x, minX, maxX);
                dialogPos.y = Mathf.Clamp(dialogPos.y, minY, maxY);
            }

            dialogWindow.anchoredPosition = dialogPos;
        }
    }
}