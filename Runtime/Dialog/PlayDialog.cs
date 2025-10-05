namespace RyanMillerGameCore.Dialog
{
    using Interactions;
    using UnityEngine;
    using UnityEngine.Events;

    public class PlayDialog : MonoBehaviour
    {
        [SerializeField] private DialogContent content;
        [SerializeField] private TriggerDialogOn triggerDialogOn = TriggerDialogOn.Interact;
        [SerializeField] private bool triggerOnlyOnce = true;
        [SerializeField] private bool disableOnComplete = true;
        [SerializeField] private UnityEvent dialogCompleteEvent;
        [SerializeField] private float cooldownTrigger = 1;
        [SerializeField] private DialogStyle dialogStyle = DialogStyle.BottomBox;
        
        private DialogPlayer _dialogPlayer;
        private Interactive _interactive;
        private bool _triggered;
        private bool _subscribed;
        private float _cooldownTime;

        private DialogPlayer DialogPlayer
        {
            get
            {
                if (_dialogPlayer != null)
                {
                    return _dialogPlayer;
                }
                if (DialogManager.Instance)
                {
                    _dialogPlayer = DialogManager.Instance.GetDialogPlayer(dialogStyle);
                }
                return _dialogPlayer;
            }
        }

        private void Start()
        {
            if (triggerDialogOn == TriggerDialogOn.Start)
            {
                TriggerDialog();
            }
        }

        public void TriggerDialog()
        {
            if (_triggered)
            {
                return;
            }
            if (Time.unscaledTime < _cooldownTime)
            {
                return;
            }
            _cooldownTime = Time.unscaledTime + cooldownTrigger;
            if (content == null)
            {
                Debug.LogError("DialogTrigger can't be played because dialog content is missing.", gameObject);
                return;
            }
            if (DialogPlayer == null)
            {
                Debug.LogError("DialogTrigger can't be played because DialogPlayer is missing.", gameObject);
                return;
            }
            
            bool playDialogSuccess = DialogPlayer.PlayDialog(content);
            if (playDialogSuccess == false)
            {
                return;
            }
            if (_interactive)
            {
                _interactive.SetSelected(false);
            }
            _triggered = true;
            _subscribed = true;
            DialogPlayer.DialogComplete += OnDialogComplete;
        }

        private void OnEnable()
        {
            if (triggerDialogOn == TriggerDialogOn.Interact)
            {
                _interactive = GetComponent<Interactive>();
                if (_interactive == null)
                {
                    _interactive = gameObject.AddComponent<Interactive>();
                }
                if (_interactive.OnInteract == null)
                {
                    _interactive.OnInteract = new UnityEvent();
                }
                _interactive.OnInteract.AddListener(TriggerDialog);
            }
        }

        private void OnDisable()
        {
            if (_subscribed && DialogPlayer)
            {
                DialogPlayer.DialogComplete -= OnDialogComplete;
                _subscribed = false;
            }
            if (_interactive != null)
            {
                _interactive.OnInteract.RemoveListener(TriggerDialog);
            }
        }

        private void OnDialogComplete()
        {
            dialogCompleteEvent?.Invoke();
            if (_subscribed)
            {
                DialogPlayer.DialogComplete -= OnDialogComplete;
                _subscribed = false;
            }
            if (disableOnComplete)
            {
                enabled = false;
                if (_interactive)
                {
                    _interactive.enabled = false;
                }
            }
            if (_interactive)
            {
                if (_interactive)
                {
                    _interactive.InteractionWasCompleted(this);
                }
            }
            if (triggerOnlyOnce == false)
            {
                _triggered = false;
            }
        }
    }

    public enum TriggerDialogOn
    {
        None = 0,
        Start = 1,
        Interact = 2
    }
}