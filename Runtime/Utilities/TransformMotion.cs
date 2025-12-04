namespace RyanMillerGameCore {
	using UnityEngine;
	using System;

	[CreateAssetMenu(fileName = "TransformMotion", menuName = "RyanMillerGameCore/TransformMotion", order = 0)]
	[Serializable]
	public class TransformMotion : ScriptableObject {
		public AnimationCurve animOnX = AnimationCurve.Linear(0, 0, 1, 1);
		public AnimationCurve animOnY = AnimationCurve.Linear(0, 0, 1, 1);
		public AnimationCurve animOnZ = AnimationCurve.Linear(0, 0, 1, 1);
		public Vector3 startOffset = Vector3.zero;
		public Vector3 endOffset = Vector3.zero;
		public float initialDelay = 0;
		public float duration = 0.5f;
		public float multiplier = 1.0f;
		public bool resetAtEnd = true;
		public bool disableAtEnd = false;
	}
}