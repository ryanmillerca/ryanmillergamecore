namespace RyanMillerGameCore.Path
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Path : MonoBehaviour
    {

        #region Methods

        public int NodeIndex(PathNode node)
        {
            if (node == null)
            {
                return -1;
            }

            return PathNodes.IndexOf(node);
        }

        public PathNode GetFirstNode()
        {
            if (PathNodes.Count == 0)
            {
                return null;
            }

            return PathNodes[0];
        }

        public PathNode GetNode(int index)
        {
            if (index < 0 || index >= PathNodes.Count)
            {
                return null;
            }

            return PathNodes[index];
        }

        public PathNode GetNextNode(PathNode node)
        {
            int index = NodeIndex(node);
            if (index == -1)
            {
                return null;
            }

            return GetNextNode(index);
        }

        public PathNode GetNextNode(int index)
        {
            if (index < 0 || index >= PathNodes.Count)
            {
                return null;
            }

            if (pathType == PathType.Loop)
            {
                return PathNodes[(index + 1) % PathNodes.Count];
            }
            else if (pathType == PathType.PingPong)
            {
                if (index == PathNodes.Count - 1)
                {
                    isReversing = true;
                }
                else if (index == 0)
                {
                    isReversing = false;
                }

                if (isReversing)
                {
                    return PathNodes[index - 1];
                }
                else
                {
                    return PathNodes[index + 1];
                }
            }
            else if (pathType == PathType.Once)
            {
                return PathNodes[Mathf.Clamp(index + 1, 0, PathNodes.Count - 1)];
            }

            return null;
        }

        #endregion


        #region Type

        private enum PathType
        {
            Once,
            Loop,
            PingPong
        }

        #endregion


        #region Properties

        public List<PathNode> PathNodes
        {
            get
            {
                if (pathNodes == null)
                {
                    pathNodes = new List<PathNode>(GetComponentsInChildren<PathNode>());
                }

                return pathNodes;
            }
        }

        #endregion


        #region Serialized Fields

        [SerializeField] private PathType pathType = PathType.Loop;

        #endregion


        #region Fields

        private List<PathNode> pathNodes;
        private bool isReversing = false;

        #endregion


        #region Editor Helper

        private void OnDrawGizmos()
        {

            List<PathNode> pathNodes = new List<PathNode>(GetComponentsInChildren<PathNode>());

            if (pathNodes == null || pathNodes.Count < 2)
            {
                return;
            }

            Gizmos.color = Color.white;

            for (int i = 0; i < pathNodes.Count; i++)
            {

                PathNode currentNode = pathNodes[i];
                PathNode nextNode = (i == pathNodes.Count - 1 && pathType == PathType.Loop)
                    ? pathNodes[0]
                    : (i < pathNodes.Count - 1 ? pathNodes[i + 1] : null);
                if (pathType != PathType.Loop && i == pathNodes.Count - 1)
                {
                    continue;
                }

                if (currentNode != null && nextNode != null)
                {
                    Gizmos.DrawLine(currentNode.transform.position, nextNode.transform.position);
                }
            }
        }

        #endregion
    }
}