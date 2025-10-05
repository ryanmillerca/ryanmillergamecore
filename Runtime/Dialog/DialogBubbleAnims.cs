namespace RyanMillerGameCore.Dialog
{
    using System.Collections;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Controls bubble fade/scale based on whether the shared dialogText has content.
    /// Opens when text is non-empty, closes when empty. When DialogPlayer signals navigation,
    /// the bubble hides immediately and will only consider opening again once navigation ends.
    /// Intended for use when DialogPlayer.controlCanvasAlpha = false so no other alpha writers interfere.
    /// </summary>
    public class DialogBubbleAnims : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Wiring")]
        [SerializeField] private DialogPlayer dialogPlayer;

        [Header("Visual Parts")]
        [SerializeField] private RectTransform stemTransform;
        [SerializeField] private RectTransform bubbleTransform;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] private float animateInTime = 0.25f;
        [SerializeField] private float animateOutTime = 0.25f;
        [SerializeField] private bool animateScale = true;
        [SerializeField] private bool scaleX = false;
        [SerializeField] private bool scaleY = true;
        [SerializeField] private bool scaleZ = false;
        [SerializeField] private bool animateFade = true;
        [SerializeField] private AnimationCurve transitionCurve = null;

        [Header("Text (single TMP used by DialogPlayer)")]
        [SerializeField] private TextMeshProUGUI dialogText;

        #endregion

        #region Private Types & State

        private enum State { Closed, Opening, Open, Closing }
        private State _state = State.Closed;

        // When true, we suppress opening even if text appears.
        private bool _navigationActive = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (transitionCurve == null)
            {
                transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }

        private void Start()
        {
            // Start hidden and subscribe
            ForceClosedVisual();
            _state = State.Closed;

            if (dialogPlayer != null)
            {
                dialogPlayer.DialogOpened       += OnDialogOpened;
                dialogPlayer.DialogClosed       += OnDialogClosed;
                dialogPlayer.DialogNextLine     += OnDialogNextLine;
                dialogPlayer.DialogTextChanged  += OnDialogTextChanged;

                // NEW: nav signals from DialogPlayer
                dialogPlayer.NavigationStarted  += OnNavigationStarted;
                dialogPlayer.NavigationEnded    += OnNavigationEnded;
            }

            // Snap to correct initial visibility
            //ApplyVisibilityFromText(snap: true);
        }

        private void OnDestroy()
        {
            if (!dialogPlayer) return;

            dialogPlayer.DialogOpened       -= OnDialogOpened;
            dialogPlayer.DialogClosed       -= OnDialogClosed;
            dialogPlayer.DialogNextLine     -= OnDialogNextLine;
            dialogPlayer.DialogTextChanged  -= OnDialogTextChanged;

            dialogPlayer.NavigationStarted  -= OnNavigationStarted;
            dialogPlayer.NavigationEnded    -= OnNavigationEnded;
        }

        #endregion

        #region Event Handlers

        private void OnDialogOpened()
        {
            // Do not force open here; text may not be set yet.
        }

        private void OnDialogNextLine()
        {
            // Text for the next line may be empty (movement-only) or delayed (after nav).
            ApplyVisibilityFromText(snap: false);
        }

        private void OnDialogTextChanged()
        {
            // During nav we suppress opening regardless of text.
            if (_navigationActive)
            {
                // Ensure hidden during nav
                PlayClose();
                return;
            }

            // Otherwise follow text content
            ApplyVisibilityFromText(snap: false);
        }

        private void OnDialogClosed()
        {
            PlayClose();
        }

        // NEW: Navigation control
        private void OnNavigationStarted()
        {
            _navigationActive = true;
            // Hide immediately, even if previous line had text
            PlayClose();
        }

        private void OnNavigationEnded()
        {
            _navigationActive = false;
            // Now it's safe to open if the line actually has text (which DialogPlayer sets after nav)
            ApplyVisibilityFromText(snap: false);
        }

        #endregion

        #region Core Logic

        private bool HasText()
        {
            return dialogText != null && !string.IsNullOrEmpty(dialogText.text);
        }

        private void ApplyVisibilityFromText(bool snap)
        {
            if (_navigationActive)
            {
                // Never open during navigation
                if (snap) { ForceClosedVisual(); } else { PlayClose(); }
                return;
            }

            if (HasText())
            {
                if (snap) { ForceOpenVisual(); } else { PlayOpen(); }
            }
            else
            {
                if (snap) { ForceClosedVisual(); } else { PlayClose(); }
            }
        }

        #endregion

        #region Visual State Setters

        private void ForceOpenVisual()
        {
            StopAllCoroutines();
            if (animateFade) canvasGroup.alpha = 1f;
            if (animateScale)
            {
                bubbleTransform.localScale = Vector3.one;
                stemTransform.localScale   = Vector3.one;
            }
            _state = State.Open;
        }

        private void ForceClosedVisual()
        {
            StopAllCoroutines();
            if (animateFade) canvasGroup.alpha = 0f;
            if (animateScale)
            {
                bubbleTransform.localScale = new Vector3(1f, 0f, 1f);
                stemTransform.localScale   = new Vector3(1f, 0f, 1f);
            }
            _state = State.Closed;
        }

        #endregion

        #region Animation Orchestrators

        private void PlayOpen()
        {
            if (_navigationActive) return;                      // gate against nav
            if (_state == State.Open || _state == State.Opening) return;

            StopAllCoroutines();
            StartCoroutine(CoOpen());
        }

        private void PlayClose()
        {
            if (_state == State.Closed || _state == State.Closing) return;

            StopAllCoroutines();
            StartCoroutine(CoClose());
        }

        #endregion

        #region Animation Coroutines

        private IEnumerator CoOpen()
        {
            _state = State.Opening;

            if (animateInTime <= 0f)
            {
                ForceOpenVisual();
                yield break;
            }

            for (float i = 0f; i <= animateInTime; i += Time.unscaledDeltaTime)
            {
                float t = transitionCurve.Evaluate(i / animateInTime);

                if (animateFade) canvasGroup.alpha = t;

                if (animateScale)
                {
                    float sx = scaleX ? t : 1f;
                    float sy = scaleY ? t : 1f;
                    float sz = scaleZ ? t : 1f;
                    stemTransform.localScale   = new Vector3(sx, sy, sz);
                    bubbleTransform.localScale = new Vector3(sx, sy, sz);
                }

                yield return new WaitForEndOfFrame();
            }

            ForceOpenVisual();
        }

        private IEnumerator CoClose()
        {
            _state = State.Closing;

            if (animateOutTime <= 0f)
            {
                ForceClosedVisual();
                yield break;
            }

            for (float i = 0f; i <= animateOutTime; i += Time.unscaledDeltaTime)
            {
                float t = transitionCurve.Evaluate(1f - i / animateOutTime);

                if (animateFade) canvasGroup.alpha = t;

                if (animateScale)
                {
                    float sx = scaleX ? t : 1f;
                    float sy = scaleY ? t : 1f;
                    float sz = scaleZ ? t : 1f;
                    stemTransform.localScale   = new Vector3(sx, sy, sz);
                    bubbleTransform.localScale = new Vector3(sx, sy, sz);
                }

                yield return new WaitForEndOfFrame();
            }

            ForceClosedVisual();
        }

        #endregion
    }
}