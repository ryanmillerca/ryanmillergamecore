namespace RyanMillerGameCore.Tiles
{
	using System;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;

    [CustomEditor(typeof(TileObjectManager))]
    public class TileObjectManagerEditor : Editor
    {
        
	    [NonSerialized] private TileObjectManager _tileObjectManager;

        private TileObjectManager TileObjectManager
        {
	        get
	        {
		        if (_tileObjectManager == null)
		        {
			        _tileObjectManager = (TileObjectManager)target;
		        }
		        return _tileObjectManager;
	        }
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10);

            if (GUILayout.Button("Regenerate Tile Objects"))
            {
	            TileObjectManager.RegeneratePrefabs();
            }
        
            // select current tile to paint with
            if (!_paintMode)
            {
    	        if (GUILayout.Button("\nPaint Mode is Off\n"))
    	        {
    		        _paintMode = true;
    	        }
            }
            else
            {
    	        Color oldColor = GUI.backgroundColor;
    	        GUI.backgroundColor = Color.yellow;
    	        if (GUILayout.Button("\nPaint Mode is On\n"))
    	        {
    		        _paintMode = false;
    		        if (_registeredSceneViewGUI == true)
    		        {
    			        SceneView.duringSceneGui -= sceneView => OnSceneGUI();
    			        _registeredSceneViewGUI  = false;
    		        }
    	        }
    	        GUI.backgroundColor = oldColor;
    	        if (_registeredSceneViewGUI == false)
    	        {
    		        _registeredSceneViewGUI  = true;
    		        SceneView.duringSceneGui += sceneView => OnSceneGUI();
    	        }
            }
        
            // flood fill
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill All"))
            {
    	        if (EditorUtility.DisplayDialog("Are you sure", "This will change ALL of your current tiles. Are you sure?", "Yes",
    		            "No"))
    	        {
		            TileObjectManager.SetAllTiles(1);
    	        }
            }

            if (GUILayout.Button("Erase All"))
            {
    	        if (EditorUtility.DisplayDialog("Are you sure", "This will erase ALL of your current tiles. Are you sure?", "Yes",
    		            "No"))
    	        {
		            TileObjectManager.SetAllTiles(0);
    	        }
            }
            GUILayout.EndHorizontal();
        }
    
        #region Painting
        
        
        [NonSerialized] private bool _registeredSceneViewGUI = false;
        [NonSerialized] private bool _paintMode = false;
        [NonSerialized] private bool _mouseWasDown = false;
        [NonSerialized] private int[] _lastTranslatedPos;

        private void OnSceneGUI() 
    	{
    		if (Application.isPlaying)
    		{
    			_mouseWasDown = false;
                _paintMode = false;
    			return;
    		}
    		if (!Selection.activeGameObject)
    		{
    			_paintMode = false;
    			return;
    		}
    		if (_paintMode)
    		{
    			if (Selection.activeGameObject != TileObjectManager.gameObject)
    			{
    				_paintMode = false;
    			}
    			Event current = Event.current;
    			int controlID = GUIUtility.GetControlID(FocusType.Passive);
    			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    			if (current.alt == false && current.button == 0)
    			{
    				int drawValue = current.shift ? 0 : 1;
    				if (current.type == EventType.MouseDown || current.type == EventType.MouseDrag)
    				{
    					_mouseWasDown = true;
    					DrawTileHere(drawValue);
    					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    				}
    				else if (_mouseWasDown && current.type == EventType.MouseUp)
    				{
					    TileObjectManager.RefreshAllTiles();
    				}
    			}
    			else if (current.type == EventType.Layout)
    			{
    				if (_mouseWasDown)
    				{
    					_mouseWasDown = false;
    					HandleUtility.AddDefaultControl(controlID);
    				}
    			}
                SceneView.RepaintAll();
            }
    	}

    	private void DrawTileHere(int tileValue) {
    		// draw a 3d plane to plot mouse clicks against and get a 3d cursor position
    		Plane gridPlane = new Plane(Vector3.up, TileObjectManager.transform.position);
    		Ray planeRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
    		float gridHit;
    		// if the click lands on the plane (it should)
    		if (gridPlane.Raycast(planeRay, out gridHit)) {
    			// get the point it hit and translate it to grid
    			Vector3 targetPoint = planeRay.GetPoint(gridHit);
    			int[] translatedPos = TileObjectManager.TileMap.PositionToGridCoord(targetPoint);
    			if (!translatedPos.Equals(_lastTranslatedPos))
    			{
				    TileObjectManager.ChangeTile(translatedPos[0], translatedPos[1], tileValue);
    				_lastTranslatedPos = translatedPos;
				    TileObjectManager.RefreshAllTiles();
    			}
    		}
    	}
	    
    	#endregion
    }
}