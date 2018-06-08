#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexTile))]
[InitializeOnLoad]
public class HexTileCoordinateHandle : Editor
{
    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    private static void DrawHandleNotSelected(HexTile tile, GizmoType gizmoType)
    {
        //Since clicking on a tile in the scene view will take us to the graphic and not the root object, we'll check if the root object is actually selected
        if (Selection.transforms
                     .Select(transform => transform.GetComponentInParent<HexTile>())
                     .Any(selectedTile => selectedTile == tile))
        {
            DrawHandleSelected(tile, GizmoType.InSelectionHierarchy);
        }
        else
        {
            //Meaning that up until cameraDistance 20, we draw everything, but from there on, every 10 distance will filter out more and more labels.
            int renderDistance = Mathf.Max(1, 
                                        SceneView.currentDrawingSceneView.camera.orthographic ?
                                            Mathf.FloorToInt(SceneView.currentDrawingSceneView.cameraDistance / 20) :
                                            Mathf.FloorToInt(SceneView.currentDrawingSceneView.cameraDistance / 10));

            if (tile.Coordinates.x % renderDistance == 0)
            {
                Handles.color = Color.black;
                Handles.Label(tile.transform.position + Vector3.left, tile.Coordinates.ToString(), EditorStyles.label);
            }
        }
    }

    [DrawGizmo(GizmoType.InSelectionHierarchy)]
    private static void DrawHandleSelected(HexTile tile, GizmoType gizmoType)
    {
        Handles.color = Color.black;
        Handles.Label(tile.transform.position + Vector3.left, tile.Coordinates.ToString(), EditorStyles.boldLabel);

        drawDistanceToSelectedTiles(tile);
    }

    /// <summary>
    /// Draws a line to all other selected tiles and indicates the distance between them.
    /// </summary>
    private static void drawDistanceToSelectedTiles(HexTile tile)
    {
        Handles.color = Color.blue;
        HexTile[] selectedTiles = Selection.transforms
                                           .Select(transform => transform.GetComponentInParent<HexTile>())
                                           .Where(selectedTile => selectedTile != tile)
                                           .ToArray();

        foreach (HexTile selectedTile in selectedTiles)
        {
            //Since each of these will be drawn twice (Once from selectedTile to tile and the other from tile to selectedTile), 
            //we'll filter all the ones going from up to down, to make sure we're only drawing once.
            if (selectedTile.transform.position.z > tile.transform.position.z ||
                (selectedTile.transform.position.z == tile.transform.position.z && selectedTile.transform.position.x > tile.transform.position.x))
            {
                Handles.DrawLine(tile.transform.position, selectedTile.transform.position);
                Handles.Label(new Vector3((tile.transform.position.x + selectedTile.transform.position.x) / 2,
                                          (tile.transform.position.y + selectedTile.transform.position.y) / 2,
                                          (tile.transform.position.z + selectedTile.transform.position.z) / 2)
                                          + Vector3.up,
                              tile.DistanceTo(selectedTile).ToString(),
                              EditorStyles.boldLabel);
            }
        }
    }
}
#endif