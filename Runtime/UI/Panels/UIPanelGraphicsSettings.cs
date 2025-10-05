namespace RyanMillerGameCore.UI
{
    using UnityEngine;
    using Settings;

    public class UIPanelGraphicsSettings : UIPanelBase
    {
        [SerializeField] private GameObject uiPanel;
        [SerializeField] private SettingsItem resolutionSettings;
        [SerializeField] private SettingsItem fullscreenSettings;
        [SerializeField] private SettingsItem vsyncSettings;
        [SerializeField] private SettingsItem fpsLimitSettings;
        [SerializeField] private SettingsItem autoQualitySettings;
        [SerializeField] private UIPanelConfirmation panelConfirmation;
        [SerializeField] private GameState stateOnClosed = GameState.Paused;
        
        public override void OnPanelOpened()
        {
            base.OnPanelOpened();
            GameStateManager.Instance.ChangeGameState(GameState.Menus);
            GraphicsSettings.Instance.LoadGraphicsSettings(false);
        }

        public void OnButtonApply()
        {
            GraphicsSettings.Instance.ApplySettings();
            if (GraphicsSettings.Instance.WillRequireConfirmation)
            {
                panelConfirmation.OpenPanel(GraphicsSettings.Instance.SaveNewSettings,
                    GraphicsSettings.Instance.ApplyOldSettings);
            }
            else
            {
                GraphicsSettings.Instance.SaveNewSettings();
            }
        }
        
        public void OnButtonBack()
        {
            GameStateManager.Instance.ChangeGameState(stateOnClosed);
            Close();
        }
        
        public void OnButtonRevert()
        {
            GraphicsSettings.Instance.RevertSettings();
        }
    }
}