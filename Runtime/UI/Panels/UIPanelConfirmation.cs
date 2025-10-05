namespace RyanMillerGameCore.UI
{
    using UnityEngine;
    using System;
    using UnityEngine.UI;
    using TMPro;

    public class UIPanelConfirmation : UIPanelBase
    {
        [SerializeField] private GameObject confirmationPanel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmationText;
        [SerializeField] private bool useCountdown = true;
        [SerializeField] private float countdownDuration = 10;
        [SerializeField] private string countDownText = "Reverting in {0} seconds";
        
        private float _currentCountDown;
        private bool _panelOpen = false;
        private Action _onConfirm;
        private Action _onCancel;

        public void OpenPanel(Action onConfirm, Action onCancel, string message = "")
        {
            base.Open();
            
            GameCanvasManager.Instance.OpenPanel(this);
            
            _onCancel = onCancel;
            _onConfirm = onConfirm;
            
            _panelOpen = true;
            confirmationPanel.SetActive(true);

            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);

            bool useCustomString = !string.IsNullOrEmpty(message);
            if (useCustomString)
            {
                confirmationText.SetText(string.Format(message, Mathf.RoundToInt(_currentCountDown)));
            }
            else
            {
                confirmationText.SetText(string.Format(countDownText, Mathf.RoundToInt(_currentCountDown)));
            }
            
            if (useCountdown)
            {
                _currentCountDown = countdownDuration;
            }
        }

        private void Update()
        {
            if (_panelOpen == false)
            {
                return;
            }
            if (useCountdown == false)
            {
                return;
            }
            if (_currentCountDown > 0)
            {
                _currentCountDown -= Time.unscaledDeltaTime;
                confirmationText.SetText(string.Format(countDownText, Mathf.RoundToInt(_currentCountDown)));
            }
            else
            {
                _onCancel?.Invoke();
                Close();
            }
        }

        private void Confirm()
        {
            _onConfirm?.Invoke();
            Close();
        }

        private void Cancel()
        {
            _onCancel?.Invoke();
            Close();
        }

        public override void Close()
        {
            _panelOpen = false;
            base.Close();
        }
    }
}