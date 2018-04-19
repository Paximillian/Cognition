#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CogEffectManager), true)]
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
        CogEffectManager manager = target as CogEffectManager;

        while (m_EffectsToRemove.Count > 0)
        {
            manager.CogEffects.Remove(m_EffectsToRemove[0]);
            DestroyImmediate(m_EffectsToRemove[0], true);
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
                CogEffectManager manager = target as CogEffectManager;

                CogEffect newEffect = manager.gameObject.AddComponent(newEffectScript.GetClass()) as CogEffect;

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
            
            MonoScript newEffectScript = EditorGUILayout.ObjectField("Add New Effect", null, typeof(MonoScript), false) as MonoScript;
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
        foreach (CogEffect effect in ((CogEffectManager)target).CogEffects
                                                               .Where(effect => effect?.Keyword == keyword))
        {
            drawEffect(effect);
        }
    }

    private void drawEffect(CogEffect effect)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            if (ShowEffect[effect] = EditorGUILayout.Foldout(ShowEffect[effect], effect.GetType().Name))
            {
                EditorGUI.indentLevel++;
                Editor.CreateEditor(effect).DrawDefaultInspector();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove"))
                    {
                        m_EffectsToRemove.Add(effect);
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