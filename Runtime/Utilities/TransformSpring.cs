namespace RyanMillerGameCore.Utilities
{

    using UnityEngine;

    public class TransformSpring : MonoBehaviour
    {
        [SerializeField] [Range(0, 1)] private float m_Damping = 0.9f;

        [Header("Target Transform")] 
        [SerializeField] private Transform m_Target;

        [Header("Spring Settings")] 
        [SerializeField] [Range(0, 1)] private float m_SpringStrength = 0.1f;


        #region Private Properties

        private Vector3 Velocity { get; set; }

        #endregion


        #region Methods

        private void Update()
        {
            if (m_Target != null)
            {
                this.Velocity += (m_Target.position - transform.position) * m_SpringStrength;
                transform.position += this.Velocity * Time.deltaTime;
                this.Velocity *= Mathf.Pow(1.0f - m_Damping, Time.deltaTime);
            }
        }

        #endregion
    }
}