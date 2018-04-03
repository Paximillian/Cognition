#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HexGridConstructionWindow : EditorWindow
{
    #region Variables
    private enum ePlacementType
    {
        Tile,
        ResourceCog,
        GoalCog,
        StartingPosition
    }
    private ePlacementType m_CurrentPlacementType = ePlacementType.Tile;

    private const float k_HexagonRadius = 20;

    /// <summary>
    /// The current open editor window, only 1 window at a time is supported.
    /// </summary>
    private static HexGridConstructionWindow s_CurrentlyOpenWindow = null;

    /// <summary>
    /// The currently loaded Hex Grid
    /// </summary>
    private static HexGrid s_HexGrid = null;

    /// <summary>
    /// The tiles that have already been renderered this frame, to make sure that we don't render the same tile twice or run into an infinite loop while renderering.
    /// </summary>
    private List<HexTile> m_TilesDrawnThisFrame = new List<HexTile>();

    /// <summary>
    /// The expansion tiles that have already been renderered this frame, to make sure that we don't render the same tile twice or run into an infinite loop while renderering.
    /// </summary>
    private List<Vector3> m_ExpansionTilesDrawnThisFrame = new List<Vector3>();

    /// <summary>
    /// How many player start positions have been drawn in this frame so far.
    /// </summary>
    private int m_PlayerStartPosesDrawnThisFrame = 0;
    private int m_GoalsDrawnThisFrame = 0;

    /// <summary>
    /// How many player start positions have been drawn during the last frame.
    /// </summary>
    private int m_PlayerStartPosesDrawnLastFrame;
    private int m_GoalsDrawnLastFrame;
    #endregion Variables

    #region AccessPoints
    /// <summary>
    /// Opens a new editor window to edit the shape of our hex grid..
    /// </summary>
    /// <param name="i_HexGrid">The serialized property representing HexGrid.</param>
    public static void Open(SerializedObject i_HexGrid)
    {
        //The editor window gets messed up during play, so we only support editting during edit.
        if (!Application.isPlaying)
        {
            HexGrid hexGrid = i_HexGrid.targetObject as HexGrid;

            //If another window is open, we replace it with the new one.
            if (s_CurrentlyOpenWindow != null && !s_HexGrid.Equals(hexGrid))
            {
                Undo.undoRedoPerformed -= s_CurrentlyOpenWindow.Repaint;
                s_CurrentlyOpenWindow.Close();
            }

            //We then open the new window.
            s_HexGrid = hexGrid;
            s_CurrentlyOpenWindow = EditorWindow.GetWindow<HexGridConstructionWindow>();
            s_CurrentlyOpenWindow.titleContent = new GUIContent($"Hex Grid Editor");

            i_HexGrid.Update();
            i_HexGrid.ApplyModifiedProperties();

            Undo.undoRedoPerformed += s_CurrentlyOpenWindow.Repaint;

            s_CurrentlyOpenWindow.setupGrid();
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Can't edit Hex Grid during Play", "Ok");
        }
    }
    #endregion AccessPoints

    #region PrivateMethods
    /// <summary>
    /// A grid object can only have HexTiles as direct children, and must have at least one.
    /// </summary>
    private void setupGrid()
    {
        //First we check if we need to remove any objects from the grid since they're not tiles.
        for(int i = 0; i < s_HexGrid.transform.childCount; ++i)
        {
            Transform child = s_HexGrid.transform.GetChild(i);
            if (child.GetComponent<HexTile>() == null)
            {
                DestroyImmediate(child.gameObject);
                i = 0;
            }
        }

        //Then, if we don't have any tiles in the grid, we create them.
        if(s_HexGrid.transform.childCount == 0)
        {
            createInitialTile();
        }
    }

    /// <summary>
    /// Creates the origin tile at the center of the world.
    /// </summary>
    private void createInitialTile()
    {
        GameObject initialTile = PrefabUtility.InstantiatePrefab(s_HexGrid.TilePrefab.gameObject) as GameObject;
        initialTile.name = initialTile.name.Replace("(Clone)", String.Empty);
        initialTile.transform.SetParent(s_HexGrid.transform);
    }

    /// <summary>
    /// Removes the given tile from the grid.
    /// </summary>
    /// <param name="i_Tile"></param>
    private void removeTile(HexTile i_Tile)
    {
        unlinkTile(i_Tile);
        DestroyImmediate(i_Tile.gameObject);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
    
    /// <summary>
    /// Creates a new tile at each currently drawn expansion spot.
    /// </summary>
    private void expandGrid()
    {
        foreach(Vector3 expansionPosition in m_ExpansionTilesDrawnThisFrame)
        {
            createTile(expansionPosition);
        }
    }

    /// <summary>
    /// Creates atile at the given editor window position.
    /// </summary>
    private void createTile(Vector3 i_Position)
    {
        //Create the tile 
        GameObject newTile = PrefabUtility.InstantiatePrefab(s_HexGrid.TilePrefab.gameObject) as GameObject;
        newTile.name = newTile.name.Replace("(Clone)", String.Empty);

        //We'll draw all the hexagons with offset from the center of the editor window.
        float worldEdgeRadius = Vector3.Distance(newTile.transform.position, newTile.transform.Find("tile_axis_Z_POS").position);
        i_Position -= new Vector3(position.width / 2, position.height / 2);
        Vector3 normalizedSpacePosition = i_Position / k_HexagonRadius;
        Vector3 tilePos = normalizedSpacePosition * worldEdgeRadius;
        tilePos = new Vector3(tilePos.x, -tilePos.z, tilePos.y);
        newTile.transform.position = tilePos;

        //Links the new tile to all its neighbours
        newTile.transform.SetParent(s_HexGrid.transform);
        linkNewTile(newTile, worldEdgeRadius);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    /// <summary>
    /// Creates a new tile as a neighbour of the given tile at the given angle.
    /// </summary>
    private void createTile(HexTile i_OriginTile, int i_Angle)
    {
        //Create the tile 
        GameObject newTile = PrefabUtility.InstantiatePrefab(s_HexGrid.TilePrefab.gameObject) as GameObject;
        newTile.name = newTile.name.Replace("(Clone)", String.Empty);

        //Finds the new tile's new position.
        Transform connectionPosToOrigin = null;
        switch (i_Angle)
        {
            case 0:
                connectionPosToOrigin = i_OriginTile.transform.Find("tile_axis_Z_POS");
                break;
            case 60:
                connectionPosToOrigin = i_OriginTile.transform.Find("tile_axis_Y_POS");
                break;
            case 120:
                connectionPosToOrigin = i_OriginTile.transform.Find("tile_axis_X_NEG");
                break;
            case 180:
                connectionPosToOrigin = i_OriginTile.transform.Find("tile_axis_Z_NEG");
                break;
            case 240:
                connectionPosToOrigin = i_OriginTile.transform.Find("tile_axis_Y_NEG");
                break;
            case 300:
                connectionPosToOrigin = i_OriginTile.transform.Find("tile_axis_X_POS");
                break;
        }

        //And places the tile object in world space.
        Vector3 newTileDirection = (i_OriginTile.transform.position - connectionPosToOrigin.position).normalized;
        float worldEdgeRadius = Vector3.Distance(i_OriginTile.transform.position, connectionPosToOrigin.position);
        newTile.transform.position = i_OriginTile.transform.position + newTileDirection * worldEdgeRadius * 2;

        //Links the new tile to all its neighbours
        newTile.transform.SetParent(s_HexGrid.transform);
        linkNewTile(newTile, worldEdgeRadius);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    /// <summary>
    /// Links the given tile to all tiles neighbouring it.
    /// </summary>
    /// <param name="i_WorldEdgeRadius">The inner radius of the hex in world space (Or distance from center of hex to center of edge).</param>
    private void linkNewTile(GameObject i_Tile, float i_WorldEdgeRadius)
    {
        HexTile tile = i_Tile.GetComponent<HexTile>();
        for (int i = 0; i < 360; i += 60)
        {
            Vector3 searchDirection = (Quaternion.AngleAxis(i, Vector3.up) * -Vector3.forward).normalized;
            Vector3 centerOfNeighbourTile = i_Tile.transform.position + searchDirection * i_WorldEdgeRadius * 2 + Vector3.up;

            RaycastHit hit;
            if (Physics.Raycast(centerOfNeighbourTile, -Vector3.up, out hit, 2, LayerMask.GetMask("HexTile")))
            {
                HexTile neighbour = hit.collider.GetComponent<HexTile>();

                Undo.RecordObjects(new UnityEngine.Object[] { tile, neighbour }, "Setup tile neighbours");
                switch (i)
                {
                    case 0:
                        tile.PositiveZNeighbour = neighbour;
                        neighbour.NegativeZNeighbour = tile;
                        break;
                    case 60:
                        tile.PositiveYNeighbour = neighbour;
                        neighbour.NegativeYNeighbour = tile;
                        break;
                    case 120:
                        tile.NegativeXNeighbour = neighbour;
                        neighbour.PositiveXNeighbour = tile;
                        break;
                    case 180:
                        tile.NegativeZNeighbour = neighbour;
                        neighbour.PositiveZNeighbour = tile;
                        break;
                    case 240:
                        tile.NegativeYNeighbour = neighbour;
                        neighbour.PositiveYNeighbour = tile;
                        break;
                    case 300:
                        tile.PositiveXNeighbour = neighbour;
                        neighbour.NegativeXNeighbour = tile;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Removes the current tile from being linked by any of its neighbours.
    /// </summary>
    private void unlinkTile(HexTile i_Tile)
    {
        if (i_Tile.NegativeXNeighbour) i_Tile.NegativeXNeighbour.PositiveXNeighbour = null;
        if (i_Tile.NegativeYNeighbour) i_Tile.NegativeYNeighbour.PositiveYNeighbour = null;
        if (i_Tile.NegativeZNeighbour) i_Tile.NegativeZNeighbour.PositiveZNeighbour = null;
        if (i_Tile.PositiveXNeighbour) i_Tile.PositiveXNeighbour.NegativeXNeighbour = null;
        if (i_Tile.PositiveYNeighbour) i_Tile.PositiveYNeighbour.NegativeYNeighbour = null;
        if (i_Tile.PositiveZNeighbour) i_Tile.PositiveZNeighbour.NegativeZNeighbour = null;
    }
    #endregion PrivateMethods

    #region GUIDrawers
    /// <summary>
    /// Draws the tiles of this grid.
    /// </summary>
    private void drawTiles()
    {
        if (s_HexGrid.transform.childCount > 0)
        {
            HexTile i_CenterTile = s_HexGrid.transform.GetChild(0).GetComponent<HexTile>();
            drawTile(i_CenterTile);
        }
    }

    /// <summary>
    /// Draws a hexagonal tile at the given space.
    /// </summary>
    private void drawTile(HexTile i_Tile)
    {
        if (!m_TilesDrawnThisFrame.Contains(i_Tile))
        {
            m_TilesDrawnThisFrame.Add(i_Tile);

            //We'll draw all the hexagons with offset from the center of the editor window.
            float worldInnerRadius = Vector3.Distance(i_Tile.transform.position, i_Tile.transform.Find("tile_axis_Z_POS").position);
            Vector3 normalizedSpacePosition = i_Tile.transform.position / worldInnerRadius;
            Vector3 tilePos = normalizedSpacePosition* k_HexagonRadius;
            tilePos = new Vector3(tilePos.x, -tilePos.z, tilePos.y);
            tilePos += new Vector3(position.width / 2, position.height / 2);

            drawActiveTile(i_Tile, tilePos);

            //Draws all spaces around this one that are possible to build a tile at.
            for (int i = 0; i < 360; i += 60)
            {
                switch (i)
                {
                    case 0:
                        if (i_Tile.PositiveZNeighbour) { drawTile(i_Tile.PositiveZNeighbour); }
                        else if (m_CurrentPlacementType == ePlacementType.Tile) { drawExpansionTile(i_Tile, i, tilePos); }
                        break;
                    case 60:
                        if (i_Tile.PositiveYNeighbour) { drawTile(i_Tile.PositiveYNeighbour); }
                        else if (m_CurrentPlacementType == ePlacementType.Tile) { drawExpansionTile(i_Tile, i, tilePos); }
                        break;
                    case 120:
                        if (i_Tile.NegativeXNeighbour) { drawTile(i_Tile.NegativeXNeighbour); }
                        else if (m_CurrentPlacementType == ePlacementType.Tile) { drawExpansionTile(i_Tile, i, tilePos); }
                        break;
                    case 180:
                        if (i_Tile.NegativeZNeighbour) { drawTile(i_Tile.NegativeZNeighbour); }
                        else if (m_CurrentPlacementType == ePlacementType.Tile) { drawExpansionTile(i_Tile, i, tilePos); }
                        break;
                    case 240:
                        if (i_Tile.NegativeYNeighbour) { drawTile(i_Tile.NegativeYNeighbour); }
                        else if (m_CurrentPlacementType == ePlacementType.Tile) { drawExpansionTile(i_Tile, i, tilePos); }
                        break;
                    case 300:
                        if (i_Tile.PositiveXNeighbour) { drawTile(i_Tile.PositiveXNeighbour); }
                        else if (m_CurrentPlacementType == ePlacementType.Tile) { drawExpansionTile(i_Tile, i, tilePos); }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Draws a filled tile in the position, that shows an active tile in the grid.
    /// </summary>
    private void drawActiveTile(HexTile i_Tile, Vector3 i_Position)
    {
        Vector3[] hexagonVertices = getVerticesFor(i_Position);

        if (i_Tile.ResidentCog == null)
        {
            Handles.color = Color.white;
        }
        else if (i_Tile.ResidentCog.GetType().Equals(s_HexGrid.ResourceCogPrefab.GetType()))
        {
            Handles.color = Color.green;
        }
        else if (i_Tile.ResidentCog.GetType().Equals(s_HexGrid.GoalCogPrefab.GetType()))
        {
            ++m_GoalsDrawnThisFrame;
            Handles.color = Color.yellow;
        }
        else if (i_Tile.ResidentCog.GetType().Equals(s_HexGrid.PlayerInitialCogPrefab.GetType()))
        {
            if (++m_PlayerStartPosesDrawnThisFrame <= 2)
            {
                Handles.color = Color.red;
            }
        }
        else if (i_Tile.ResidentCog.GetType().Equals(s_HexGrid.PlayerInitialCogPrefab.GetType()))
        {
            Handles.color = Color.blue;
        }

        Handles.DrawAAConvexPolygon(hexagonVertices);

        Handles.color = Color.black;
        Handles.DrawAAPolyLine(hexagonVertices);

        if (s_HexGrid.transform.childCount > 1)
        {
            //If we're currently placing tiles
            if (m_CurrentPlacementType == ePlacementType.Tile)
            {
                //Draws a button to remove the tile at this space.
                if (GUI.Button(new Rect(i_Position - new Vector3(1, 1, 0) * k_HexagonRadius / 2, Vector2.one * k_HexagonRadius), " -", EditorStyles.boldLabel))
                {
                    removeTile(i_Tile);
                }
            }
        }
        //Otherwise, we'll be given the choice to build or remove a cog on this tile.
        if (m_CurrentPlacementType != ePlacementType.Tile)
        {
            //If we don't currently have a cog, we'll be able to build one here.
            if (i_Tile.ResidentCog == null)
            {
                if ((m_CurrentPlacementType == ePlacementType.StartingPosition && m_PlayerStartPosesDrawnLastFrame >= 2) ||
                    (m_CurrentPlacementType == ePlacementType.GoalCog && m_GoalsDrawnLastFrame >= 1))
                {
                }
                else
                {
                    if (GUI.Button(new Rect(i_Position - new Vector3(1, 1, 0) * k_HexagonRadius / 2, Vector2.one * k_HexagonRadius), " +", EditorStyles.boldLabel))
                    {
                        Cog cogToBuild = null;

                        switch (m_CurrentPlacementType)
                        {
                            case ePlacementType.ResourceCog:
                                cogToBuild = s_HexGrid.ResourceCogPrefab;
                                break;
                            case ePlacementType.GoalCog:
                                cogToBuild = s_HexGrid.GoalCogPrefab;
                                break;
                            case ePlacementType.StartingPosition:
                                cogToBuild = s_HexGrid.PlayerInitialCogPrefab;
                                break;
                        }

                        Cog cog = Editor_BuildCog(i_Tile, cogToBuild);
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    }
                }
            }
            //Otherwise, we'll be able to remove the cog on this space.
            else
            {
                if (GUI.Button(new Rect(i_Position - new Vector3(1, 1, 0) * k_HexagonRadius / 2, Vector2.one * k_HexagonRadius), " -", EditorStyles.boldLabel))
                {
                    Editor_DestroyCog(i_Tile);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
        }
    }

    /// <summary>
    /// Draws an empty tile at the given position, that represents a tile where we can expand our grid to.
    /// </summary>
    private void drawExpansionTile(HexTile i_Tile, int i_Angle, Vector3 i_TileEditorPos)
    {
        Vector3 expansionTileCenterPosition = Quaternion.AngleAxis(i_Angle, Vector3.forward) * Vector3.up * k_HexagonRadius * 2 + i_TileEditorPos;
        if (!m_ExpansionTilesDrawnThisFrame.Any(position => vector3Approximately(expansionTileCenterPosition, position)))
        {
            m_ExpansionTilesDrawnThisFrame.Add(expansionTileCenterPosition);

            Vector3[] hexagonVertices = getVerticesFor(expansionTileCenterPosition);

            Handles.color = Color.blue;
            Handles.DrawAAPolyLine(hexagonVertices);

            if (GUI.Button(new Rect(expansionTileCenterPosition - new Vector3(1, 1, 0) * k_HexagonRadius / 2, Vector2.one * k_HexagonRadius), " +", EditorStyles.boldLabel))
            {
                createTile(i_Tile, i_Angle);
            }
        }
    }
    
    private bool vector3Approximately(Vector3 a, Vector3 b)
    {
        return mathfApproximately(a.x, b.x) &&
               mathfApproximately(a.y, b.y) &&
               mathfApproximately(a.z, b.z);
    }

    private bool mathfApproximately(float a, float b)
    {
        return Mathf.Abs(a-b) <= 0.01f;
    }

    /// <summary>
    /// Draws controls for the entire grid.
    /// </summary>
    private void drawGridControls()
    {
        switch (m_CurrentPlacementType)
        {
            case ePlacementType.GoalCog:
                GUI.color = Color.yellow;
                break;
            case ePlacementType.ResourceCog:
                GUI.color = Color.green;
                break;
            case ePlacementType.StartingPosition:
                GUI.color = Color.red;
                break;
        }

        GUILayout.BeginVertical();
        {
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Placing: ");

                if(m_CurrentPlacementType == ePlacementType.Tile) { GUI.enabled = false; }
                if (GUILayout.Button("Tile"))
                {
                    m_CurrentPlacementType = ePlacementType.Tile;
                }

                GUI.enabled = true; if (m_CurrentPlacementType == ePlacementType.ResourceCog) { GUI.enabled = false; }
                if (GUILayout.Button("Resource"))
                {
                    m_CurrentPlacementType = ePlacementType.ResourceCog;
                }

                GUI.enabled = true; if (m_CurrentPlacementType == ePlacementType.GoalCog) { GUI.enabled = false; }
                if (GUILayout.Button("Goal"))
                {
                    m_CurrentPlacementType = ePlacementType.GoalCog;
                }

                GUI.enabled = true; if (m_CurrentPlacementType == ePlacementType.StartingPosition) { GUI.enabled = false; }
                if (GUILayout.Button("Player"))
                {
                    m_CurrentPlacementType = ePlacementType.StartingPosition;
                }

                GUILayout.FlexibleSpace();
                if (m_CurrentPlacementType == ePlacementType.Tile && GUILayout.Button("Expand Grid"))
                {
                    expandGrid();
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// Gets the vertices that make up the hexagon at the given position.
    /// </summary>
    private Vector3[] getVerticesFor(Vector3 i_Position)
    {
        Vector3[] hexagonVertices = new Vector3[7];

        for (int i = 0; i < 6; ++i)
        {
            hexagonVertices[i] = Quaternion.AngleAxis(i * 60 + 30, Vector3.forward) * Vector3.up * k_HexagonRadius + i_Position;
        }

        hexagonVertices[6] = hexagonVertices[0];

        return hexagonVertices;
    }
    #endregion GUIDrawers

    #region UnityMethods
    /// <summary>
    /// The editor window's content gets messed up under certain conditions, so we make sure to close it when such happen.
    /// </summary>
    private void checkForCloseWindowConditions()
    {
        //When we start play mode.
        if (Application.isPlaying)
        {
            Close();
        }
        //When a new scene loads or a compilation is made (In which case, the message list variable will be reset.
        else if (s_HexGrid == null)
        {
            Close();
        }
    }

    private void OnGUI()
    {
        m_GoalsDrawnLastFrame = m_GoalsDrawnThisFrame;
        m_GoalsDrawnThisFrame = 0;
        m_PlayerStartPosesDrawnLastFrame = m_PlayerStartPosesDrawnThisFrame;
        m_PlayerStartPosesDrawnThisFrame = 0;
        m_TilesDrawnThisFrame.Clear();
        m_ExpansionTilesDrawnThisFrame.Clear();

        if (s_HexGrid)
        {
            drawTiles();

            drawGridControls();
        }
    }

    private void Update()
    {
        checkForCloseWindowConditions();
    }
    #endregion UnityMethods

    #region EditorMethods
    public static Cog Editor_BuildCog(HexTile i_Tile, Cog i_CogPrefab)
    {
        Cog cog = null;
        if (i_Tile.ResidentCog == null)
        {
            cog = (UnityEditor.PrefabUtility.InstantiatePrefab(i_CogPrefab.gameObject) as GameObject).GetComponent<Cog>();
            cog.transform.position = i_Tile.transform.position;
            cog.HoldingTile = i_Tile;

            UnityEditor.Undo.RecordObject(i_Tile, "Updated resident cog");
            i_Tile.ResidentCog = cog;

            cog.transform.position += Vector3.up * (i_Tile.transform.Find("tile_cog_connection").position.y - cog.transform.Find("tile_cog_connection").position.y);
        }
        return cog;
    }

    public static void Editor_DestroyCog(HexTile i_Tile)
    {
        DestroyImmediate(i_Tile.ResidentCog.gameObject);
        i_Tile.ResidentCog = null;
    }
    #endregion EditorMethods
}
#endif
