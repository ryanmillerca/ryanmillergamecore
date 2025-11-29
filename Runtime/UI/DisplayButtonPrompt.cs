namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using Character;
    using UI;
    
    [RequireComponent(typeof(Interactive))]
    public class DisplayButtonPrompt : MonoBehaviour {
        [SerializeField] private Vector3 promptOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private PromptAction _promptAction;

        private Interactive _interactive;
        private Collider _targetCollider;
        private PromptData _promptData;
        private Vector3 _transformOffset;
        
        private void Awake()
        {
            _targetCollider = GetComponent<Collider>();
            _transformOffset = GetOffset();
            _promptData = new PromptData
            {
                targetTransform = transform,
                transformOffset = _transformOffset,
                promptAction = _promptAction
            };
        }
        
        private void OnEnable()
        {
            _interactive = GetComponent<Interactive>();
            if (_interactive == null)
            {
                _interactive = gameObject.AddComponent<Interactive>();
            }
            _interactive.WasSelected += InteractiveOnWasSelected;
        }

        private void InteractiveOnWasSelected(bool obj)
        {
            if (obj)
            {
                ShowPrompt();
            }
            else
            {
                HidePrompt();
            }
        }

        private void OnDisable()
        {
            if (UIButtonPrompt.Instance)
            {
                UIButtonPrompt.Instance.TryHidePrompt(_promptData);
            }
            if (_interactive != null)
            {
                _interactive.WasSelected -= InteractiveOnWasSelected;
            }
        }

        private void HidePrompt()
        {
            if (UIButtonPrompt.Instance) {
                UIButtonPrompt.Instance.TryHidePrompt(_promptData);
            }
            else {
                Debug.LogError("You need to have a UIButtonPrompt object in the scene.");
            }
        }

        private void ShowPrompt()
        {
            if (UIButtonPrompt.Instance) {
                UIButtonPrompt.Instance.TryDisplayPrompt(_promptData);
            }
            else {
                Debug.LogError("You need to have a UIButtonPrompt object in the scene.");
            }
        }
        
        private Vector3 GetOffset()
        {
            Vector3 offset = promptOffset;
            if (_targetCollider != null)
            {
                offset.y += _targetCollider.bounds.extents.y;
            }
            return offset;
        }
    }
}