namespace RyanMillerGameCore.Camera
{
    using UnityEngine;

    public class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera Instance { get; private set; }

        public Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponentInChildren<Camera>();
                }
                return _camera;
            }
        }

        private Camera _camera;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}