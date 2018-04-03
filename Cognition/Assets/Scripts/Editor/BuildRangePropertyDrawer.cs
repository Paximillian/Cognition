#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BuildRangeAttribute))]
public class BuildRangePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        BuildRangeAttribute range = attribute as BuildRangeAttribute;

        if (property.intValue != int.MaxValue)
        {
            EditorGUI.IntSlider(new Rect(position.x, position.y, position.width * (5f / 6f) - 10, position.height),
                                property,
                                range.Min,
                                range.Max);
        }
        else
        {
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width * (5f / 6f) - 10, position.height),
                        property.displayName);
        }

        property.intValue = EditorGUI.ToggleLeft(new Rect(position.x + position.width * (5f/6f), position.y, position.width / 6 - 5, position.height),
                                                 "Infinity",
                                                 property.intValue == int.MaxValue) 
                                     ? int.MaxValue : Mathf.Min(property.intValue, range.Max);

        property.serializedObject.ApplyModifiedProperties();
    }
}
#endif