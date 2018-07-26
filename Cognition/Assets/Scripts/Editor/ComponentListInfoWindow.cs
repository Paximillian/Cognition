#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ComponentListInfoWindow : EditorWindow
{
    private Component[] m_Components;
    private int m_SelectedNetworkId;

    [MenuItem("Window/Component List Info")]
    public static void Open()
    {
        ComponentListInfoWindow.GetWindow<ComponentListInfoWindow>();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            {
                GameObject objectToCheck = EditorGUILayout.ObjectField("Object to check", null, typeof(GameObject), allowSceneObjects: true) as GameObject;

                if (objectToCheck)
                {
                    m_Components = objectToCheck.GetComponents<Component>();
                }

                if (m_Components != null)
                {
                    foreach (Component comp in m_Components)
                    {
                        GUILayout.Label(comp.ToString());
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();
    }
}
#endif