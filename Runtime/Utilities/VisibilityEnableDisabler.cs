namespace RyanMiller.Scripts.Utilities
{
    using UnityEngine;

    /// <summary>
    /// Simple util that uses OnBecameVisible and OnBecameInvisible to enable/disable MonoBehaviours.
    /// </summary>
    public class VisibilityEnableDisabler : MonoBehaviour {
        public MonoBehaviour[] scriptsToToggle;

        void OnBecameVisible() {
            foreach (MonoBehaviour scriptToToggle in scriptsToToggle)
            {
                scriptToToggle.enabled = true;
            }
        }

        void OnBecameInvisible() {
            foreach (MonoBehaviour scriptToToggle in scriptsToToggle)
            {
                scriptToToggle.enabled = false;
            }
        }
    }
}