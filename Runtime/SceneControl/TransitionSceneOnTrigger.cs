namespace RyanMillerGameCore.SceneControl
{
    using UnityEngine;
    
    public class TransitionSceneOnTrigger : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad;
        [SerializeField, Tooltip("Optional: Use Location ID instead (takes priority)")] private LocationID targetLocationID;
        
        [ContextMenu("Trigger")]
        public void Trigger()
        {
            SceneTransitioner.Instance.FadeToScene(sceneToLoad);
        }
    }
}