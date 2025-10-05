namespace RyanMillerGameCore.UI
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>Base class for all full-screen or modal UI panels.</summary>
    public abstract class UIPanelBase : MonoBehaviour
    {
        [Header("Panel Content")] [SerializeField]
        private GameObject panelContents;

        [Header("UI Navigation")] [SerializeField]
        private GameObject defaultSelectable;

        [SerializeField] private bool registerFocus = true;

        public bool IsOpen => panelContents != null && panelContents.activeSelf;

        protected virtual void Awake()
        {
            if (panelContents == null && transform.childCount > 0)
            {
                panelContents = transform.GetChild(0).gameObject;
            }
        }

        /// <summary>
        /// Call this to open the panel with optional animation or transition
        /// </summary>
        public virtual void Open()
        {
            if (IsOpen)
            {
                return;
            }
            OnPanelOpened();
            GameCanvasManager.Instance.OpenPanel(this);
        }

        /// <summary>
        /// Call this to close the panel with optional animation or transition
        /// </summary>
        public virtual void Close()
        {
            if (!IsOpen)
            {
                return;
            }
            OnPanelClosed(); 
            GameCanvasManager.Instance.ClosePanel(this);
        }

        public virtual void OnPanelBecameTop()
        {
            SetDefaultSelectable();
        }
        
        /// <summary>
        /// Should only be used by GameCanvasManager
        /// </summary>
        public virtual void OnPanelOpened()
        {
            if (!registerFocus)
            {
                return;
            }
            HandleShow();
            SetDefaultSelectable();
        }


        public virtual void OnPanelClosed()
        {
            HandleHide();
        }

        protected virtual void HandleShow()
        {
            panelContents.SetActive(true);
        }

        protected virtual void HandleHide()
        {
            panelContents.SetActive(false); 
        }

        protected virtual void SetDefaultSelectable()
        {
            if (defaultSelectable != null && defaultSelectable.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(defaultSelectable);
            }
        }
    }
}