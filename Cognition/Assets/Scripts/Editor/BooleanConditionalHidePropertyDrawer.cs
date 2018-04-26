using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BooleanConditionalHideAttribute))]
public class BooleanConditionalHidePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        BooleanConditionalHideAttribute conditionalHideAttribiute = (BooleanConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(conditionalHideAttribiute, property);
        bool wasEnabled = GUI.enabled;

        GUI.enabled = enabled;
        if (!conditionalHideAttribiute.HideInInspector || enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        BooleanConditionalHideAttribute conditionalHideAttribiute = (BooleanConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(conditionalHideAttribiute, property);

        if (!conditionalHideAttribiute.HideInInspector || enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool GetConditionalHideAttributeResult(BooleanConditionalHideAttribute conditionalHideAttribiute, SerializedProperty property)
    {
        bool enabled = true;
        //returns the property path of the property we want to apply the attribute to
        string propertyPath = property.propertyPath;
        //changes the path to the conditionalsource property path
        string conditionPath = propertyPath.Replace(property.name, conditionalHideAttribiute.ConditionalSourceField);
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            enabled = sourcePropertyValue.boolValue;
        }
        else
        {
            Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + conditionalHideAttribiute.ConditionalSourceField);
        }

        return enabled;
    }
}