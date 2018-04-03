#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkObjectFinderWindow : EditorWindow
{
    private int m_SelectedNetworkId;

    [MenuItem("Window/Network Object Finder")]
    public static void Open()
    {
        NetworkObjectFinderWindow.GetWindow<NetworkObjectFinderWindow>();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            {
                m_SelectedNetworkId = EditorGUILayout.IntField("ID", m_SelectedNetworkId);

                if (GUILayout.Button("Find"))
                {
                    GameObject localObject = ClientScene.FindLocalObject(new NetworkInstanceId((uint)m_SelectedNetworkId));

                    if (localObject)
                    {
                        EditorGUIUtility.PingObject(localObject.GetInstanceID());
                        Selection.activeObject = localObject;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", $"Can't find object with ID {m_SelectedNetworkId}", "Ok");
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