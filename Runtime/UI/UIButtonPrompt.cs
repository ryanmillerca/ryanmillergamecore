namespace RyanMillerGameCore.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;
    using Camera;
    using Character;

    [RequireComponent(typeof(RectTransform))]
    public class UIButtonPrompt : Singleton<UIButtonPrompt>
    {
        [SerializeField] private List<PromptData> activePrompts = new List<PromptData>();
        [SerializeField] private float maxDistance = 2f;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private PerPlatformPrompt perPlatformPrompt;
        [SerializeField] private Image promptImage; 
        [SerializeField] private UpdatePromptMode updatePromptMode = UpdatePromptMode.LateUpdate;
        
        private enum UpdatePromptMode
        {
            LateUpdate,
            OnChanged
        }
        
        private PromptData _activePrompt;
        private RectTransform _rectTransform;
        private bool _promptAvailable;
        private Canvas _parentCanvas;
        private Renderer _targetRenderer;
        private Camera _playerCameraComponent;
        private PlayerCharacter _player;
        private int _numActivePrompts;

        public void OnButtonPress()
        {
            var player = CharacterManager.Instance.Player;
            if (player)
            {
                player.References._characterInput.FireInteractFromUI();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!Application.isPlaying)
            {
                return;
            }
            if (perPlatformPrompt)
            {
                perPlatformPrompt.OnPromptGraphicsChanged -= PromptGraphicsChanged;
            }
        }

        private void PromptGraphicsChanged(PromptGraphics obj)
        {
            if (_promptAvailable)
            {
                promptImage.sprite = perPlatformPrompt.GetSpriteFor(_activePrompt.promptAction);
            }
        }

        public void TryDisplayPrompt(PromptData promptData)
        {
            if (activePrompts.Contains(promptData) == false)
            {
                activePrompts.Add(promptData);
                if (updatePromptMode == UpdatePromptMode.OnChanged)
                {
                    DecideWhichPromptToShow();
                }
            }
        }
        
        public void TryHidePrompt(PromptData promptData)
        {
            if (activePrompts.Contains(promptData))
            {
                activePrompts.Remove(promptData);
                if (updatePromptMode == UpdatePromptMode.OnChanged)
                {
                    DecideWhichPromptToShow();
                }
            }
        }

        private Camera PlayerCameraComponent
        {
            get
            {
                if (_playerCameraComponent == null)
                {
                    _playerCameraComponent = PlayerCamera.Instance.Camera;
                }

                return _playerCameraComponent;
            }
        }
        
        private Vector3 playerComparePosition
        {
            get
            {
                if (_player == null)
                {
                    _player = PlayerCharacter.Instance;
                }
                return _player.GetFrontPosition();
            }
        }
        
        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();
            HidePrompt();
            perPlatformPrompt.OnPromptGraphicsChanged += PromptGraphicsChanged;
        }

        private void LateUpdate()
        {
            if (updatePromptMode == UpdatePromptMode.LateUpdate)
            {
                DecideWhichPromptToShow();
            }

            if (_promptAvailable) 
            {
                UpdatePromptPosition();
            }
        }
        
        private void DecideWhichPromptToShow()
        {
            // if there are none, then hide all
            if (activePrompts.Count == 0)
            {
                HidePrompt();
                return;
            }
            
            // find closest prompt
            PromptData closestPrompt = null;
            float closestDist = float.MaxValue;
            foreach (PromptData prompt in activePrompts)
            {
                float distToThisPrompt = GetDistanceToPrompt(prompt);
                if (distToThisPrompt < maxDistance)
                {
                    if (distToThisPrompt < closestDist)
                    {
                        closestPrompt = prompt;
                        closestDist = distToThisPrompt;
                    }
                }
            }

            // there may be no closest prompt, in which case you should hide
            if (closestPrompt == null)
            {
                HidePrompt();
            }
            // otherwise update ref to closest prompt
            else
            {
                _activePrompt = closestPrompt;
                UpdateActivePrompt(_activePrompt);
            }
        }

        private float GetDistanceToPrompt(PromptData promptData)
        {
            return Vector3.Distance(playerComparePosition, promptData.targetTransform.position);
        }

        private void UpdateActivePrompt(PromptData promptData)
        {
            promptImage.sprite = perPlatformPrompt.GetSpriteFor(promptData.promptAction);
            _promptAvailable = true;
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
        }

        private void HidePrompt()
        {
            _activePrompt = null;
            _promptAvailable = false;
            if (canvasGroup)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
            }
        }

        private void UpdatePromptPosition()
        {
            if (_activePrompt.targetTransform == null)
            {
                activePrompts.Remove(_activePrompt);
                HidePrompt();
                return;
            }
            
            // Convert world position to screen space
            Vector3 screenPosition = PlayerCameraComponent.WorldToScreenPoint(_activePrompt.targetTransform.position + _activePrompt.transformOffset);

            // Convert screen position to canvas position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.transform as RectTransform,
                screenPosition,
                _parentCanvas.worldCamera,
                out var canvasPosition);

            // Apply the position to the UI element
            _rectTransform.localPosition = canvasPosition;
        }
    }
    
    public class PromptData
    {
        public Transform targetTransform;
        public Vector3 transformOffset;
        public PromptAction promptAction;
    }

    public enum PromptAction
    {
        Move = 0, 
        Look = 1,
        Attack = 2,
        Interact = 3,
        Dodge = 4,
        Special = 5,
        Pause = 6
    }
}