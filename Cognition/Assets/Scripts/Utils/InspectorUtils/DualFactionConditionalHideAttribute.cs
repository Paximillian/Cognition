using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class DualFactionConditionalHideAttribute : PropertyAttribute
{
    /// <summary>
    /// The name of the bool field that will be in control.
    /// </summary>
    private string m_conditionalSourceField = "";

    /// <summary>
    /// Hide in inspector / FALSE = Disable in inspector.
    /// </summary>
    private bool m_hideInInspector = true;

    public string ConditionalSourceField
    { get { return m_conditionalSourceField; } }

    public bool HideInInspector
    { get { return m_hideInInspector; } }

    public DualFactionConditionalHideAttribute(string conditionalSourceField, bool hideInInspector = true)
    {
        this.m_conditionalSourceField = conditionalSourceField;
        this.m_hideInInspector = hideInInspector;
    }
}