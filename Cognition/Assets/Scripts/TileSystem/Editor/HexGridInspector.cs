#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(HexGrid))]
public class HexGridInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginVertical();
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("      Edit Hex Grid      "))
                {
                    HexGridConstructionWindow.Open(serializedObject);
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("     Clear Hex Grid     "))
                {
                    GameObject[] tilesAndCogs = GameObject.FindObjectsOfType<HexTile>().Select(tile => tile.gameObject)
                                                                                       .Union(GameObject.FindObjectsOfType<Cog>()
                                                                                                        .Select(cog => cog.gameObject))
                                                                                       .ToArray();

                    for (int i = 0; i < tilesAndCogs.Length; ++i)
                    {
                        DestroyImmediate(tilesAndCogs[i]);
                    }
                    
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }
        GUILayout.EndVertical();
    }
}
#endif