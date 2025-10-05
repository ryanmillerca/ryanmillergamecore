namespace RyanMillerGameCore.Path
{
	using System.Collections;
	using UnityEngine;

	public class PathFollower : MonoBehaviour
	{

		#region Public Methods

		public void Begin()
		{
			StopAllCoroutines(); // in case someone calls begin again while it's pathing, just reset it for now
			PathNode startNode = this.Path.GetFirstNode();
			StartCoroutine(MoveBetweenNodesAsync(startNode, this.Path.GetNextNode(startNode)));
		}

		[ContextMenu("Pause")]
		public void Pause()
		{
			this.Paused = true;
		}

		[ContextMenu("Resume")]
		public void Resume()
		{
			this.Paused = false;
		}

		#endregion


		#region Serialized Fields

		[Header("Path")] [SerializeField] private Path m_Path;


		[Header("Follow Movement")] [SerializeField]
		private float m_Speed;

		[SerializeField] [Range(0, 10)] private float m_WaitTimeAtEachNode = 0.5f;


		[Header("Options")] [SerializeField] private bool m_BeginOnStart = true;

		#endregion


		#region Private Properties

		private Path Path
		{
			get
			{
				if (m_Path == null)
				{
					m_Path = GetComponentInChildren<Path>();
				}

				return m_Path;
			}
		}

		private bool Paused { get; set; }

		#endregion


		#region MonoBehaviour

		private void Start()
		{
			if (m_BeginOnStart)
			{
				Begin();
			}
		}

		#endregion


		#region Pivate Methods

		private IEnumerator MoveBetweenNodesAsync(PathNode currentNode, PathNode nextNode)
		{

			transform.position = currentNode.Position;

			while (Vector3.SqrMagnitude(transform.position - nextNode.Position) > 0.05f)
			{
				if (!this.Paused)
				{
					transform.position =
						Vector3.MoveTowards(transform.position, nextNode.Position, Time.deltaTime * m_Speed);
				}

				yield return null;
			}

			transform.position = nextNode.Position;

			yield return new WaitForSeconds(m_WaitTimeAtEachNode);
			StartCoroutine(MoveBetweenNodesAsync(nextNode, this.Path.GetNextNode(nextNode)));
		}

		#endregion


		#region Editor Helper

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1, 0, 0, 0.5f);
			Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
		}

		#endregion
	}
}