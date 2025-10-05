namespace RyanMillerGameCore.LevelToys
{
    using UnityEngine;

    public class AutoRotate : MonoBehaviour
    {
        [SerializeField] private Vector3 angularVelocity;

        private Rigidbody myRigidbody;

        private void Awake()
        {
            myRigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            myRigidbody.angularVelocity = angularVelocity;
        }
    }
}