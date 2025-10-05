namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;

    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateInstance();
                }
                return _instance;
            }
        }

        private static void CreateInstance()
        {
            if (_instance != null) return;

            GameObject runnerObject = new GameObject("CoroutineRunner");
            _instance = runnerObject.AddComponent<CoroutineRunner>();
            DontDestroyOnLoad(runnerObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}