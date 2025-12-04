namespace RyanMillerGameCore.Animation {
	using UnityEngine;

	public static class AnimatorExtensions {
		public static bool HasParameter(this Animator animator, string paramName) {
			if (animator == null || string.IsNullOrEmpty(paramName)) {
				return false;
			}

			var parameters = animator.parameters;
			foreach (AnimatorControllerParameter t in parameters) {
				if (t.name == paramName) {
					return true;
				}
			}

			return false;
		}
	}
}
