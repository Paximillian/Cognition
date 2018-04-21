#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CogEffectManager), true)]
[CanEditMultipleObjects]
public class CogEffectManagerEditor : Editor
{
    #region Variables
    /// <summary>
    /// Just a way to show the default field that shows which script is being inspected.
    /// </summary>
    private MonoScript m_InspectedScript;

    /// <summary>
    /// Is the foldout displaying all effect categories open?
    /// </summary>
    private bool m_ShowEffects = true;

    /// <summary>
    /// Holds a value indicating if each of the effect categories is expanded right now.
    /// </summary>
    private Dictionary<eCogEffectKeyword, bool> m_ShowEffectCategory;
    private Dictionary<eCogEffectKeyword, bool> ShowEffectCategory
    {
        get
        {
            if (m_ShowEffectCategory == null)
            {
                m_ShowEffectCategory = new Dictionary<eCogEffectKeyword, bool>();

                foreach (eCogEffectKeyword keyword in Enum.GetValues(typeof(eCogEffectKeyword)))
                {
                    m_ShowEffectCategory.Add(keyword, true);
                }
            }

            return m_ShowEffectCategory;
        }
    }

    /// <summary>
    /// Holds a value indicating if each of the effect is expanded right now.
    /// </summary>
    private Dictionary<CogEffect, bool> m_ShowEffect;

    /// <summary>
    /// Effects that need to be removed at the end of the current frame.
    /// </summary>
    private List<CogEffect> m_EffectsToRemove = new List<CogEffect>();

    private Dictionary<CogEffect, bool> ShowEffect
    {
        get
        {
            if (m_ShowEffect == null)
            {
                m_ShowEffect = new Dictionary<CogEffect, bool>();

                foreach (CogEffect effect in (target as MonoBehaviour).GetComponents<CogEffect>())
                {
                    m_ShowEffect.Add(effect, false);
                    effect.hideFlags |= HideFlags.HideInInspector;
                }
            }

            return m_ShowEffect;
        }
    }
    #endregion Variables

    #region UnityMethods
    private void OnEnable()
    {
        m_InspectedScript = MonoScript.FromMonoBehaviour((CogEffectManager)target);
    }

    public override void OnInspectorGUI()
    {
        CogEffectManager manager = target as CogEffectManager;
        for (int i = 0; i < manager.CogEffects.Count; ++i)
        {
            if (manager.CogEffects[i] == null)
            {
                manager.CogEffects.RemoveAt(i--);
            }
        }

        drawGUI();
    }
    #endregion UnityMethods

    #region PrivateMethods
    /// <summary>
    /// Removes any effects marked for removal during the last frame.
    /// </summary>
    private void removeSelectedEffects()
    {
        while (m_EffectsToRemove.Count > 0)
        {
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                CogEffectManager manager = selectedObject.GetComponent<CogEffectManager>();

                if (manager)
                {
                    SerializedObject serializedTarget = new SerializedObject(manager);
                    serializedTarget.Update();

                    manager.CogEffects.Remove(m_EffectsToRemove[0]);
                    DestroyImmediate(m_EffectsToRemove[0], true);

                    serializedTarget.ApplyModifiedProperties();
                }
            }

            m_EffectsToRemove.RemoveAt(0);
        }
    }

    /// <summary>
    /// Checks if any new effects have been added.
    /// </summary>
    private void checkForNewEffects(MonoScript newEffectScript)
    {
        if (typeof(CogEffect).IsAssignableFrom(newEffectScript.GetClass()))
        {
            if (newEffectScript.GetClass().IsAbstract)
            {
                EditorUtility.DisplayDialog("Can't add script", $"Can't add script behaviour {newEffectScript.name}. The script class can't be abstract!", "OK");
            }
            else
            {
                foreach (GameObject selectedObject in Selection.gameObjects)
                {
                    CogEffectManager manager = selectedObject.GetComponent<CogEffectManager>();

                    if (manager)
                    {
                        SerializedObject serializedTarget = new SerializedObject(selectedObject);
                        serializedTarget.Update();

                        CogEffect newEffect = selectedObject.AddComponent(newEffectScript.GetClass()) as CogEffect;

                        if (newEffect != null)
                        {
                            newEffect.hideFlags |= HideFlags.HideInInspector;
                            manager.CogEffects.Add(newEffect);

                            if (m_ShowEffect == null)
                            {
                                ShowEffect[newEffect] = true;
                            }
                            else
                            {
                                ShowEffect.Add(newEffect, true);
                            }
                        }

                        serializedTarget.ApplyModifiedProperties();
                    }
                }
            }
        }
    }
    #endregion PrivateMethods

    #region GUIMethods
    private void drawGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script ", m_InspectedScript, typeof(MonoScript), false);
            GUI.enabled = true;

            if (m_ShowEffects = EditorGUILayout.Foldout(m_ShowEffects, "Cog Effects"))
            {
                drawEffectCategories();
            }
            
            MonoScript newEffectScript = EditorGUILayout.ObjectField(new GUIContent("Add New Effect", "Drag a script of a CogEffect here to add it to the manager"), null, typeof(MonoScript), false) as MonoScript;
            if (newEffectScript)
            {
                checkForNewEffects(newEffectScript);
            }
        }
        EditorGUILayout.EndVertical();

        removeSelectedEffects();
    }

    private void drawEffectCategories()
    {
        EditorGUI.indentLevel++;
        foreach (eCogEffectKeyword keyword in Enum.GetValues(typeof(eCogEffectKeyword)))
        {
            drawEffectCategory(keyword);
        }
        EditorGUI.indentLevel--;
    }

    private void drawEffectCategory(eCogEffectKeyword keyword)
    {
        if (ShowEffectCategory[keyword] = EditorGUILayout.Foldout(ShowEffectCategory[keyword], keyword.ToString()))
        {
            EditorGUI.indentLevel++;
            drawEffectsOfCategory(keyword);
            EditorGUI.indentLevel--;
        }
    }

    private void drawEffectsOfCategory(eCogEffectKeyword keyword)
    {
        //This a list of all selected object's CogEffects lists that match the given keyword.
        List<List<CogEffect>> targetManagerEffects = Selection.gameObjects
                                               .Select(selectedObject => selectedObject.GetComponent<CogEffectManager>().CogEffects
                                                    .Where(effect => effect?.Keyword == keyword).ToList())
                                               .ToList();

        //If all the selected managers have the same number of items in this category.
        if (targetManagerEffects.Max(effects => effects.Count) == targetManagerEffects.Min(effects => effects.Count))
        {
            for (int i = 0; i < targetManagerEffects[0].Count; ++i)
            {
                //Since we're observing the effects of all selected cogs, we'll need to find the one refering to this current object.
                CogEffect targetObjectEffect = targetManagerEffects.First(cogEffectList => ShowEffect.ContainsKey(cogEffectList[i]))
                                                                   .ToArray()[i];

                drawEffect(targetObjectEffect, 
                           targetManagerEffects.Select(effects => effects[i]).ToArray());
            }
        }
        else
        {
            GUILayout.Label("Selected objects don't have the same effects for this keyword");
        }
    }

    /// <summary>
    /// Draws the effect provided as a parameter.
    /// </summary>
    /// <param name="effect">The effect we want to draw.</param>
    /// <param name="effectOnAllSelectedManagers">An array</param>
    /// <param name="o_RequestRemoval">Outputs an indication telling us whether we need to remove this effect from the manager.</param>
    private void drawEffect(CogEffect effect, CogEffect[] effectOnAllSelectedManagers)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            if (ShowEffect[effect] = EditorGUILayout.Foldout(ShowEffect[effect], effect.GetType().GetDisplayName().Replace("Cog Effect", string.Empty)))
            {
                EditorGUI.indentLevel++;
                Editor.CreateEditor(effectOnAllSelectedManagers).OnInspectorGUI();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove"))
                    {
                        m_EffectsToRemove.AddRange(effectOnAllSelectedManagers);
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
        }
        EditorGUILayout.EndVertical();
    }
    #endregion GUIMethods
}
#endif