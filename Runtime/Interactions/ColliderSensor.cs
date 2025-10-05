namespace RyanMillerGameCore.Interactions {
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using Utilities;
	using Character;
	using System.Linq;

	public class ColliderSensor : MonoBehaviour {


		#region Events

		public event Action<Collider> ObjectEnteredSensor;
		public event Action<Collider> ObjectExitedSensor;

		#endregion


		#region Properties

		public bool SensorOccupied {
			get { return CollidersInSensor.Count > 0; }
		}

		#endregion


		#region Configuration

		[SerializeField] private ColliderSensorMode sensorMode = ColliderSensorMode.TriggerEnterExit;
		[SerializeField] private SensorConfig sensorConfig = SensorConfig.Player;
#if UNITY_EDITOR
		public List<Collider> editorDisplayObjectsInSensor = new List<Collider>();
#endif
		[SerializeField] private string[] allowedTags;
		[SerializeField] LayerMask allowedLayers;
		[SerializeField] private bool detectOnlyPlayer = false;
		[SerializeField] private bool detectSelf = false;
		[SerializeField] private float scanInterval = 0.25f;
		[SerializeField] private float heightTolerance = 0;
		[SerializeField] private bool includeTriggers = false;

		[Header("Unity Events")] [Foldout("Unity Events"), SerializeField]
		protected UnityEvent<Collider> objectEnteredSensor;

		[Foldout("Unity Events"), SerializeField]
		protected UnityEvent<Collider> objectExitedSensor;

		private float _overlapSphereRadius;
		private Vector3 _overlapSphereOffset;
		private float _nextScanTime;
		private readonly Collider[] _overlapResults = new Collider[10];
		private readonly HashSet<Collider> _scanCache = new HashSet<Collider>();

		private void ConfigureForPlayerDetection() {
			detectSelf = false;
			detectOnlyPlayer = true;
			Collider colliderComponent = GetComponent<Collider>();
			if (colliderComponent) {
				colliderComponent.isTrigger = true;
			}

			Rigidbody rigidbodyComponent = GetComponent<Rigidbody>();
			gameObject.layer = LayerMask.NameToLayer("Sensor");
			if (rigidbodyComponent) {
				rigidbodyComponent.useGravity = false;
				rigidbodyComponent.isKinematic = true;
			}
		}

		#endregion


		#region State

		protected HashSet<Collider> CollidersInSensor = new HashSet<Collider>();

		#endregion


		#region Public Methods

		/// <summary>
		/// Returns all colliders currently in the sensor.
		/// </summary>
		public HashSet<Collider> GetColliders() {
			return CollidersInSensor;
		}

		/// <summary>
		/// Returns all colliders currently in the sensor as a list.
		/// </summary>
		public List<Collider> GetCollidersAsList() {
			return new List<Collider>(CollidersInSensor);
		}

		/// <summary>
		/// Returns all colliders currently in the sensor as an array.
		/// </summary>
		public Collider[] GetCollidersAsArray() {
			Collider[] colliders = new Collider[CollidersInSensor.Count];
			CollidersInSensor.CopyTo(colliders);
			return colliders;
		}

		public Collider GetFirstItemInSensor() {
			if (CollidersInSensor.Count > 0) {
				foreach (var c in CollidersInSensor) {
					if (c != null) {
						return c;
					}
				}
			}

			return null;
		}

		#endregion


		#region Protected Methods

		protected Collider[] FilterColliders(Collider[] colliders, bool requireActiveGameObject = false, bool requireEnabled = false) {
			return colliders.Where(c =>
			c != null &&
			(!requireActiveGameObject || c.gameObject.activeInHierarchy) &&
			(!requireEnabled || c.enabled)
			).ToArray();
		}

		protected void FilterHashSetColliders(HashSet<Collider> colliders, bool requireActive = false, bool requireEnabled = false) {
			var toRemove = new List<Collider>();
			foreach (var c in colliders) {
				if (c == null ||
				    (requireActive && !c.gameObject.activeInHierarchy) ||
				    (requireEnabled && !c.enabled)) {
					toRemove.Add(c);
				}
			}

			foreach (var c in toRemove) {
				colliders.Remove(c);
			}
		}

		protected void FilterListColliders(List<Collider> colliders, bool requireActive = false, bool requireEnabled = false) {
			for (int i = colliders.Count - 1; i >= 0; i--) {
				var c = colliders[i];
				if (c == null ||
				    (requireActive && !c.gameObject.activeInHierarchy) ||
				    (requireEnabled && !c.enabled)) {
					colliders.RemoveAt(i);
				}
			}
		}

		#endregion


		#region Unity Callbacks

		private void Start() {
			ConfigureSensor();
		}

		private void ConfigureSensor() {
			Rigidbody myRigidbody = GetComponent<Rigidbody>();
			SphereCollider sphereCollider = GetComponent<SphereCollider>();
			detectOnlyPlayer = sensorConfig == SensorConfig.Player;

			// Configure the sensor based on the mode
			if (sensorMode == ColliderSensorMode.TriggerEnterExit) {
				if (sphereCollider) {
					sphereCollider.isTrigger = true;
				}
				gameObject.layer = LayerMask.NameToLayer("Sensor");
				if (myRigidbody) {
					myRigidbody.useGravity = false;
					myRigidbody.isKinematic = true;
				}
			}
			else if (sensorMode == ColliderSensorMode.OverlapSphere) {
				if (sphereCollider) {
					_overlapSphereRadius = sphereCollider.radius;
					_overlapSphereOffset = sphereCollider.center;
					sphereCollider.enabled = false;
					if (sensorConfig is SensorConfig.Character or SensorConfig.Player) {
						allowedLayers = 1 << LayerMask.NameToLayer("Character");
					}
					else if (sensorConfig == SensorConfig.Interactives) {
						allowedLayers = 1 << LayerMask.NameToLayer("Interactive");
					}
				}
				if (myRigidbody) {
					Destroy(myRigidbody);
				}
			}
		}

		private void LateUpdate() {
			if (sensorMode == ColliderSensorMode.OverlapSphere) {
				if (Time.time > _nextScanTime) {
					_nextScanTime = Time.time + scanInterval;
					ScanWithOverlapSphere();
				}
			}
		}

		private void OnTriggerEnter(Collider other) {
			if (sensorMode == ColliderSensorMode.TriggerEnterExit) {
				AddItem(other);
			}
		}

		private void OnTriggerExit(Collider other) {
			if (sensorMode == ColliderSensorMode.TriggerEnterExit) {
				RemoveItem(other);
			}
		}

		#endregion


		#region Sensor Logic

		private void ScanWithOverlapSphere() {
			Array.Clear(_overlapResults, 0, _overlapResults.Length);
			_scanCache.Clear();

			int hitCount = Physics.OverlapSphereNonAlloc(transform.position + _overlapSphereOffset, _overlapSphereRadius, _overlapResults, allowedLayers);

			for (int i = 0; i < hitCount; i++) {
				Collider hit = _overlapResults[i];
				if (includeTriggers == false && hit.isTrigger) {
					continue;
				}
				if (hit != null) {
					_scanCache.Add(hit);

					// Check height manually before adding
					if (!IsWithinHeightTolerance(hit)) {
						// If it's in the sensor but no longer valid, remove it
						if (CollidersInSensor.Contains(hit)) {
							RemoveItem(hit);
						}
						continue;
					}

					AddItem(hit); // Still does other checks (tag, duplication, etc.)
				}
			}

			// Remove stale items no longer in overlap
			foreach (var c in CollidersInSensor.ToList()) {
				if (!_scanCache.Contains(c)) {
					RemoveItem(c);
				}
			}

			if (hitCount == _overlapResults.Length) {
				Debug.LogWarning("Overlap array might be full (" + hitCount + "/" + _overlapResults.Length + "). Consider increasing the size.", gameObject);
			}
		}

		private bool TagMatch(Collider c) {
			if (allowedTags == null) {
				return true;
			}
			if (allowedTags.Length == 0) {
				return true;
			}
			foreach (var thisTag in allowedTags) {
				if (c.CompareTag(thisTag)) {
					return true;
				}
			}
			return false;
		}

		protected virtual void AddToSensor(Collider c) {
			if (CollidersInSensor.Add(c)) {
				if (!detectSelf && c.transform.root == transform.root) {
					return;
				}

				AddWatcherToCollider(c);
				FireObjectEnteredEvents(c);

#if UNITY_EDITOR
				editorDisplayObjectsInSensor.Add(c);
#endif
				ColliderListChanged();
			}
		}

		private void AddWatcherToCollider(Collider c) {
			var watcher = c.GetComponent<SensorWatcher>();
			if (watcher == null) {
				watcher = c.gameObject.AddComponent<SensorWatcher>();
			}
			watcher.onDisabled += RemoveItem;
		}

		protected virtual void FireObjectEnteredEvents(Collider c) {
			objectEnteredSensor?.Invoke(c);
			ObjectEnteredSensor?.Invoke(c);
			ItemEnteredTrigger(c);
		}

		protected virtual void AddItem(Collider c) {
			// Check if collider is in allowedTags
			if (TagMatch(c) == false) {
				return;
			}

			// Check if collider is player
			if (detectOnlyPlayer) {
				ICharacter character = c.GetComponent<ICharacter>();
				if (character == null || !character.IsPlayer()) {
					return;
				}
			}

			// check if this already exists in collidersInSensor list
			if (CollidersInSensor.Contains(c)) {
				return;
			}

			// add it!
			AddToSensor(c);
		}

		private bool IsWithinHeightTolerance(Collider c) {
			if (heightTolerance <= 0) {
				return true;
			}
			Bounds bounds = c.bounds;
			float top = bounds.max.y;
			float bottom = bounds.min.y;
			float sensorY = transform.position.y;
			return !(bottom > sensorY + heightTolerance || top < sensorY - heightTolerance);
		}

		protected void RemoveItem(Collider c) {
			if (c == null) {
				return;
			}
			if (CollidersInSensor.Contains(c) == false) {
				return;
			}
			if (CollidersInSensor.Remove(c)) {
				// fire events
				objectExitedSensor?.Invoke(c);
				ObjectExitedSensor?.Invoke(c);
				ItemExitedTrigger(c);

				// remove sensorwatcher
				var watcher = c.GetComponent<SensorWatcher>();
				if (watcher) {
					watcher.onDisabled -= RemoveItem;
				}
#if UNITY_EDITOR
				editorDisplayObjectsInSensor.Remove(c);
#endif
				ColliderListChanged();
			}
		}

		#endregion


		#region Hooks

		protected virtual void ItemEnteredTrigger(Collider item) { }

		protected virtual void ItemExitedTrigger(Collider item) { }

		protected virtual void ColliderListChanged() {
			FilterHashSetColliders(CollidersInSensor, requireActive: true, requireEnabled: true);
            #if UNITY_EDITOR
			FilterListColliders(editorDisplayObjectsInSensor, requireActive: true, requireEnabled: true);
            #endif
		}

		#endregion


		private enum SensorConfig {
			None = 0,
			Player = 1,
			Character = 2,
			Interactives = 3
		}

		private enum ColliderSensorMode {
			Disabled = 0,
			TriggerEnterExit = 1,
			OverlapSphere = 2,
		}
	}
}
