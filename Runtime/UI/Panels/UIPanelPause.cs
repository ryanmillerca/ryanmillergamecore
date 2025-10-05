namespace RyanMillerGameCore.UI
{
    using SceneControl;
    using UnityEngine;
    
    public class UIPanelPause : UIPanelBase
    {
        [SerializeField] private UIPanelConfirmation panelQuitConfirmation;
        [SerializeField] private UIPanelConfirmation panelTitleConfirmation;

        public void OnButtonResume()
        {
            GameStateManager.Instance.TogglePause();
        }

        public void OnButtonSettings()
        {
            GameCanvasManager.Instance.OpenMenuSettings();
        }

        public void OnButtonTitle()
        {
            panelTitleConfirmation.OpenPanel(TitleYes, TitleNo);
        }

        public void TitleYes()
        {
            SceneTransitioner.Instance.FadeToScene("Title");
        }

        public void TitleNo()
        {
        }
        
        public void OnButtonQuit()
        {
            panelQuitConfirmation.OpenPanel(QuitYes, QuitNo);
        }

        public void QuitYes()
        {
            Debug.Log("Quitting Application.");
            Application.Quit();
        }

        public void QuitNo()
        {
            Debug.Log("Quitting Cancelled!");
        }
    }
}