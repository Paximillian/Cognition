using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(NetworkObjectPoolManager))]
[Serializable]
public class NetworkObjectPoolManagerEditor : Editor
{
    [SerializeField]
    [Range(0, 1000)]
    private int m_ListSize = 0;

    [SerializeField]
    private bool m_PoolListsExpanded = true;
    [SerializeField]
    private List<bool> m_SubPoolListsExpanded = new List<bool>() { false };
    [SerializeField]
    private const bool v_AllowSceneObjects = true;


    [SerializeField]
    private SerializedProperty m_ObjectPoolNames;
    [SerializeField]
    private SerializedProperty m_ObjectsToPool;
    [SerializeField]
    private SerializedProperty m_ObjectPoolStartAmounts;
    [SerializeField]
    private SerializedProperty m_SubListSizes;

    void OnEnable()
    {
        m_ObjectPoolNames = serializedObject.FindProperty("ObjectPoolNames");
        m_ObjectsToPool = serializedObject.FindProperty("ObjectsToPool");
        m_ObjectPoolStartAmounts = serializedObject.FindProperty("ObjectPoolStartAmounts");
        m_SubListSizes = serializedObject.FindProperty("SubListSizes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

		EditorGUIUtility.fieldWidth = 40;
		EditorGUIUtility.labelWidth = 80;
        EditorGUI.indentLevel = 0;

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();

        m_ListSize = EditorGUILayout.IntField("Amount of pools", m_ObjectPoolNames.arraySize);

        EditorGUILayout.EndHorizontal();

        if (m_ListSize != m_ObjectPoolNames.arraySize)
        {
            m_ObjectPoolNames.arraySize = m_ListSize;
            m_ObjectsToPool.arraySize = m_ListSize;
            m_ObjectPoolStartAmounts.arraySize = m_ListSize;
            m_SubListSizes.arraySize = m_ListSize;
        }

        if (m_PoolListsExpanded = EditorGUILayout.Foldout(m_PoolListsExpanded, "Object Pool"))
        {
            for (int i = 0; i < m_ListSize; ++i)
            {
                EditorGUI.indentLevel = 1;

                EditorGUILayout.BeginHorizontal();

                if (m_ObjectPoolNames.arraySize > i)
				{
					EditorGUILayout.PropertyField(m_ObjectPoolNames.GetArrayElementAtIndex(i), new GUIContent("Pool Tag"));
                }
                else
                {
                    m_ObjectPoolNames.InsertArrayElementAtIndex(m_ObjectPoolNames.arraySize);
                    m_ObjectPoolNames.GetArrayElementAtIndex(m_ObjectPoolNames.arraySize - 1).enumValueIndex = 0;
                }

                m_SubListSizes.GetArrayElementAtIndex(i).intValue = EditorGUILayout.IntField("Sources", m_ObjectsToPool.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").arraySize);
                
                if (m_SubListSizes.GetArrayElementAtIndex(i).intValue != m_ObjectsToPool.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").arraySize)
                {
                    m_ObjectPoolStartAmounts.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").arraySize = m_SubListSizes.GetArrayElementAtIndex(i).intValue;
                    m_ObjectsToPool.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").arraySize = m_SubListSizes.GetArrayElementAtIndex(i).intValue;
                }


                EditorGUILayout.EndHorizontal();

                while (m_SubPoolListsExpanded.Count <= i)
                {
                    m_SubPoolListsExpanded.Add(false);
                }
                if (m_SubPoolListsExpanded[i] = EditorGUILayout.Foldout(m_SubPoolListsExpanded[i], "Pooled Objects"))
                {
                    EditorGUI.indentLevel = 2;

                    for (int j = 0; j < m_ObjectsToPool.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").arraySize; ++j)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        if (m_ObjectsToPool.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").arraySize > j)
                        {
                            EditorGUILayout.PropertyField(m_ObjectsToPool.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").GetArrayElementAtIndex(j), new GUIContent("Prefab"));
                        }

                        if (m_ObjectPoolStartAmounts.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").arraySize > j)
                        {
                            m_ObjectPoolStartAmounts.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").GetArrayElementAtIndex(j).intValue = EditorGUILayout.IntField("Amount", m_ObjectPoolStartAmounts.GetArrayElementAtIndex(i).FindPropertyRelative("InnerList").GetArrayElementAtIndex(j).intValue);
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();
                }

                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.EndVertical();

        if (!Application.isPlaying)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
