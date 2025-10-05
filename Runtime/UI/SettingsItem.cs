namespace RyanMillerGameCore.UI
{
    using TMPro;
    using UnityEngine;
    using Settings;
    
    public class SettingsItem : MonoBehaviour
    {
        public TextMeshProUGUI valueLabel;

        protected virtual void OnEnable()
        {
            GraphicsSettings.Instance.GraphicsSettingsChanged += RefreshSettings;
        }
        
        private void OnDisable()
        {
            if (GraphicsSettings.Instance)
            {
                GraphicsSettings.Instance.GraphicsSettingsChanged -= RefreshSettings;
            }
        }
        
        protected virtual void RefreshSettings(GraphicsSettingsData newSettings)
        {
        }

        public void SetLabel(string labelText)
        {
            valueLabel.SetText(labelText);
        }

        public virtual void WasClicked()
        {
            
        }
    }
}