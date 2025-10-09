namespace RyanMillerGameCore.Tiles {
	using UnityEngine;
	using UnityEngine.InputSystem;

	[DefaultExecutionOrder(50)]
	public class RuntimeTilemapEditor : MonoBehaviour {
		[Header("Scene References")]
		[SerializeField] private TileObjectManager tileObjectManager;
		[SerializeField] private Camera worldCamera;

		[Header("Input (UI Action Map)")]
		[SerializeField] private InputActionReference uiPoint;
		[SerializeField] private InputActionReference uiPlaceTile;
		[SerializeField] private InputActionReference uiRemoveTile;

		[Header("Painting Settings")]
		[SerializeField] private int paintValue = 1;
		[SerializeField] private int eraseValue = 0;
		[SerializeField] private bool continuousWhileHeld = true;

		private Vector2 _screenPos;
		private bool _placeTileHeld;
		private bool _removeTileHeld;
		private Vector2Int _lastCell = new Vector2Int(int.MinValue, int.MinValue);

		private TileMap TileMap => tileObjectManager != null ? tileObjectManager.TileMap : null;

		private void OnEnable() {
			if (uiPoint != null) {
				uiPoint.action.Enable();
				uiPoint.action.performed += OnPointPerformed;
			}
			if (uiPlaceTile != null) {
				uiPlaceTile.action.Enable();
				uiPlaceTile.action.performed += OnLeftDown;
				uiPlaceTile.action.canceled += OnLeftUp;
			}
			if (uiRemoveTile != null) {
				uiRemoveTile.action.Enable();
				uiRemoveTile.action.performed += OnRightDown;
				uiRemoveTile.action.canceled += OnRightUp;
			}
		}

		private void OnDisable() {
			if (uiPoint != null) {
				uiPoint.action.performed -= OnPointPerformed;
			}
			if (uiPlaceTile != null) {
				uiPlaceTile.action.performed -= OnLeftDown;
				uiPlaceTile.action.canceled -= OnLeftUp;
			}
			if (uiRemoveTile != null) {
				uiRemoveTile.action.performed -= OnRightDown;
				uiRemoveTile.action.canceled -= OnRightUp;
			}
		}

		private void Update() {
			if (!worldCamera || !tileObjectManager || !TileMap) {
				return;
			}

			// Also poll current button state to support Input System variations
			_placeTileHeld = uiPlaceTile && uiPlaceTile.action.IsPressed();
			_removeTileHeld = uiRemoveTile && uiRemoveTile.action.IsPressed();

			if (!_placeTileHeld && !_removeTileHeld) {
				// reset last visited cell when not painting to avoid re-skipping first cell
				_lastCell = new Vector2Int(int.MinValue, int.MinValue);
				return;
			}

			if (!continuousWhileHeld && _lastCell.x != int.MinValue)
			{
				return; // single-tap mode: already did one cell this press
			}

			if (TryGetCellUnderCursor(out var cell)) {
				if (cell != _lastCell) {
					int value = _placeTileHeld ? paintValue : (_removeTileHeld ? eraseValue : paintValue);
					tileObjectManager.ChangeTile(cell.x, cell.y, value);
					tileObjectManager.RefreshAllTiles(); // trigger autotiling
					_lastCell = cell;
				}
			}
		}

		private void OnPointPerformed(InputAction.CallbackContext ctx) {
			_screenPos = ctx.ReadValue<Vector2>();
		}

		private void OnLeftDown(InputAction.CallbackContext _) {
			_placeTileHeld = true;
			_removeTileHeld = false; // prefer left if both fire
			PaintOnceAtCursor(paintValue);
		}

		private void OnLeftUp(InputAction.CallbackContext _) {
			_placeTileHeld = false;
			_lastCell = new Vector2Int(int.MinValue, int.MinValue);
		}

		private void OnRightDown(InputAction.CallbackContext _) {
			_removeTileHeld = true;
			_placeTileHeld = false;
			PaintOnceAtCursor(eraseValue);
		}

		private void OnRightUp(InputAction.CallbackContext _) {
			_removeTileHeld = false;
			_lastCell = new Vector2Int(int.MinValue, int.MinValue);
		}

		private void PaintOnceAtCursor(int value) {
			if (TryGetCellUnderCursor(out var cell)) {
				if (cell != _lastCell) {
					tileObjectManager.ChangeTile(cell.x, cell.y, value);
					tileObjectManager.RefreshAllTiles();
					_lastCell = cell;
				}
			}
		}

		/// <summary>
		/// Converts the current cursor screen position to a grid cell using the same plane logic
		/// as the editor tool (Plane facing up through the TileObjectManager origin).
		/// </summary>
		private bool TryGetCellUnderCursor(out Vector2Int cell) {
			cell = default;
			if (!worldCamera || !TileMap) {
				return false;
			}

			// Build a plane at the grid origin, facing up (Y+)
			Plane gridPlane = new Plane(Vector3.up, tileObjectManager.transform.position);
			Ray ray = worldCamera.ScreenPointToRay(_screenPos);
			if (!gridPlane.Raycast(ray, out float enter)) {
				return false;
			}
			Vector3 hitPoint = ray.GetPoint(enter);
			int[] coord = TileMap.PositionToGridCoord(hitPoint);
			cell = new Vector2Int(coord[0], coord[1]);

			// Validate against map bounds to avoid errors
			return TileMap.CoordIsValid(cell.x, cell.y);
		}

#if UNITY_EDITOR
		private void OnDrawGizmosSelected() {
			// Visualize the hovered cell for convenience during development
			if (Application.isPlaying && TryGetCellUnderCursor(out var cell)) {
				Vector3 world = tileObjectManager.transform.position +
				                new Vector3(cell.x * tileObjectManager.transform.localScale.x,
					                0f,
					                cell.y * tileObjectManager.transform.localScale.z);
				Gizmos.color = new Color(1f, 1f, 1f, 0.75f);
				Gizmos.DrawWireCube(world, new Vector3(1f, 0f, 1f));
			}
		}
#endif
	}
}
