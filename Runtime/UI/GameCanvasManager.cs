namespace RyanMillerGameCore.UI
{
    using System.Collections.Generic;
    using UnityEngine;

    public class GameCanvasManager : Singleton<GameCanvasManager>
    {
        [SerializeField] private List<UIPanelBase> panelStack = new();
        [SerializeField] private UIPanelBase uiPanelSettings;
        [SerializeField] private UIPanelBase uiPanelPause;

        public void OpenMenuSettings()
        {
            uiPanelSettings.Open();
        }

        /// <summary>Open (or bring to front) a panel.</summary>
        public void OpenPanel(UIPanelBase panel)
        {
            if (panel == null)
            {
                return;
            }

            // If already open, move to top; otherwise push.
            panelStack.Remove(panel);
            panelStack.Add(panel);

            panel.gameObject.SetActive(true);
            panel.OnPanelOpened(); // first-time logic (animations, init, etc.)
            panel.OnPanelBecameTop(); // ALWAYS fire, even on re-open
        }

        /// <summary>
        /// Close a panel.  
        /// If it was the top panel, the one beneath becomes active.
        /// </summary>
        public void ClosePanel(UIPanelBase panel)
        {
            if (panelStack.Contains(panel) == false)
            {
                Debug.LogError("Can't close a Panel that isn't in the PanelStack (" + panel.gameObject.name + ")",
                    panel.gameObject);
                return;
            }

            int indexOfPanel = panelStack.IndexOf(panel);
            int previousIndex = indexOfPanel - 1;
            bool wasTop = indexOfPanel == panelStack.Count - 1;
            
            if (previousIndex >= 0 && wasTop && panelStack.Count > 0)
            {
                panelStack[previousIndex].OnPanelBecameTop();
            }

            panelStack.Remove(panel);
        }

        public void CloseTopPanel()
        {
            if (panelStack.Count > 0)
            {
                ClosePanel(panelStack[^1]);
            }
        }

        public UIPanelBase CurrentTop => panelStack.Count > 0 ? panelStack[^1] : null;

        private void OnEnable()
        {
            if (GameStateManager.Instance)
            {
                GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance)
            {
                GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState == GameState.Paused)
            {
                uiPanelPause.Open();
            }
            else if (gameState == GameState.Gameplay)
            {
                uiPanelPause.Close();
            }
        }
    }
}