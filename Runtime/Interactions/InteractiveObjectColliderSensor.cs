namespace RyanMillerGameCore.Interactions {
	using UnityEngine;
	using System.Linq;

	public class InteractiveObjectColliderSensor : ColliderSensor {

		private IInteractive m_currentInteractive;
		public IInteractive CurrentInteractive {
			get {
				return m_currentInteractive;
			}
		}

		protected override void ItemEnteredTrigger(Collider item) {
			IInteractive interactive = item.GetComponent<IInteractive>();
			if ((Component)interactive) {
				if (interactive.enabled == false) {
					return;
				}

				DigestInteractives();

				if ((Component)m_currentInteractive) {
					m_currentInteractive.SetSelected(true);
				}
			}
		}

		/// <summary>
		/// Sorts all the interactives in the collider by distance, closest first
		/// </summary>
		private void DigestInteractives() {
			if ((Component)m_currentInteractive) {
				m_currentInteractive.SetSelected(false);
			}

			Collider[] colliders = GetCollidersAsArray();
			colliders = FilterColliders(colliders, requireActiveGameObject: true, requireEnabled: true);

			var interactives = colliders
				.Select(c => c.GetComponent<IInteractive>())
				.Where(i => i != null && i.enabled && i.gameObject.activeInHierarchy)
				.OrderBy(i => Vector3.Distance(transform.position, i.transform.position))
				.ToList();

			m_currentInteractive = interactives.FirstOrDefault();
		}


		protected override void ItemExitedTrigger(Collider item) {
			IInteractive interactive = item.GetComponent<IInteractive>();
			if ((Component)interactive) {
				interactive.SetSelected(false);
				if (interactive == m_currentInteractive) {
					DigestInteractives();
				}
			}
		}
	}
}
