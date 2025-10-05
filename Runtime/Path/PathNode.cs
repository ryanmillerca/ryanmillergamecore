namespace RyanMillerGameCore.Path
{
    using UnityEngine;

    public class PathNode : MonoBehaviour
    {

        public Vector3 Position
        {
            get { return transform.position; }
        }


        #region Editor Helper

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }

        #endregion
    }
}