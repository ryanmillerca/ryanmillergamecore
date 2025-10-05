namespace RyanMillerGameCore.Path
{
    using System.Collections;
    using UnityEngine;

    public class DelayPathFollowerOnTrigger : MonoBehaviour
    {
        #region Public Methods

        [ContextMenu("Trigger")]
        public void Trigger()
        {
            if (!isPaused)
            {
                StartCoroutine(PauseAndResume());
            }
        }

        #endregion


        #region Fields

        [SerializeField] private PathFollower pathFollower;

        [SerializeField] private float pauseDuration = 2f;

        #endregion


        #region Private Fields

        private bool isPaused = false;

        #endregion


        #region Private Methods

        private IEnumerator PauseAndResume()
        {
            isPaused = true;
            pathFollower.Pause();
            yield return new WaitForSeconds(pauseDuration);
            pathFollower.Resume();
            isPaused = false;
        }

        #endregion
    }
}