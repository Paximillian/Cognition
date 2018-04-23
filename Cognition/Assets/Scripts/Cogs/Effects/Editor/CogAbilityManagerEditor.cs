#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CogAbilityManager), true)]
[CanEditMultipleObjects]
public class CogAbilityManagerEditor : Editor
{
    #region Variables
    /// <summary>
    /// Just a way to show the default field that shows which script is being inspected.
    /// </summary>
    private MonoScript m_InspectedScript;

    /// <summary>
    /// Is the foldout displaying all ability categories open?
    /// </summary>
    private bool m_ShowAbilities = true;

    /// <summary>
    /// Holds a value indicating if each of the ability categories is expanded right now.
    /// </summary>
    private Dictionary<eCogAbilityKeyword, bool> m_ShowAbilityCategory;
    private Dictionary<eCogAbilityKeyword, bool> ShowAbilityCategory
    {
        get
        {
            if (m_ShowAbilityCategory == null)
            {
                m_ShowAbilityCategory = new Dictionary<eCogAbilityKeyword, bool>();

                foreach (eCogAbilityKeyword keyword in Enum.GetValues(typeof(eCogAbilityKeyword)))
                {
                    m_ShowAbilityCategory.Add(keyword, true);
                }
            }

            return m_ShowAbilityCategory;
        }
    }

    /// <summary>
    /// Holds a value indicating if each of the abilities is expanded right now.
    /// </summary>
    private Dictionary<CogAbility, bool> m_ShowAbility;

    /// <summary>
    /// Abilities that need to be removed at the end of the current frame.
    /// </summary>
    private List<CogAbility> m_AbilitiesToRemove = new List<CogAbility>();

    private Dictionary<CogAbility, bool> ShowAbility
    {
        get
        {
            if (m_ShowAbility == null)
            {
                m_ShowAbility = new Dictionary<CogAbility, bool>();

                foreach (CogAbility ability in (target as MonoBehaviour).GetComponents<CogAbility>())
                {
                    m_ShowAbility.Add(ability, true);
                    ability.hideFlags |= HideFlags.HideInInspector;
                }
            }

            return m_ShowAbility;
        }
    }
    #endregion Variables

    #region UnityMethods
    private void OnEnable()
    {
        m_InspectedScript = MonoScript.FromMonoBehaviour((CogAbilityManager)target);
    }

    public override void OnInspectorGUI()
    {
        CogAbilityManager manager = target as CogAbilityManager;
        for (int i = 0; i < manager.CogAbilities.Count; ++i)
        {
            if (manager.CogAbilities[i] == null)
            {
                manager.CogAbilities.RemoveAt(i--);
            }
        }

        drawGUI();
    }
    #endregion UnityMethods

    #region PrivateMethods
    /// <summary>
    /// Removes any abilities marked for removal during the last frame.
    /// </summary>
    private void removeSelectedAbilities()
    {
        while (m_AbilitiesToRemove.Count > 0)
        {
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                CogAbilityManager manager = selectedObject.GetComponent<CogAbilityManager>();

                if (manager)
                {
                    SerializedObject serializedTarget = new SerializedObject(manager);
                    serializedTarget.Update();

                    manager.CogAbilities.Remove(m_AbilitiesToRemove[0]);
                    DestroyImmediate(m_AbilitiesToRemove[0], true);

                    serializedTarget.ApplyModifiedProperties();
                }
            }

            m_AbilitiesToRemove.RemoveAt(0);
        }
    }

    /// <summary>
    /// Checks if any new abilities have been added.
    /// </summary>
    private void checkForNewAbilities(MonoScript newAbilityScript)
    {
        if (typeof(CogAbility).IsAssignableFrom(newAbilityScript.GetClass()))
        {
            if (newAbilityScript.GetClass().IsAbstract)
            {
                EditorUtility.DisplayDialog("Can't add script", $"Can't add script behaviour {newAbilityScript.name}. The script class can't be abstract!", "OK");
            }
            else
            {
                foreach (GameObject selectedObject in Selection.gameObjects)
                {
                    CogAbilityManager manager = selectedObject.GetComponent<CogAbilityManager>();

                    if (manager)
                    {
                        SerializedObject serializedTarget = new SerializedObject(selectedObject);
                        serializedTarget.Update();

                        CogAbility newAbility = selectedObject.AddComponent(newAbilityScript.GetClass()) as CogAbility;

                        if (newAbility != null)
                        {
                            newAbility.hideFlags |= HideFlags.HideInInspector;
                            manager.CogAbilities.Add(newAbility);

                            if (m_ShowAbility == null)
                            {
                                ShowAbility[newAbility] = true;
                            }
                            else
                            {
                                ShowAbility.Add(newAbility, true);
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

            if ((target as CogAbilityManager).CogAbilities.Count > 0)
            {
                if (m_ShowAbilities = EditorGUILayout.Foldout(m_ShowAbilities, "Cog Abilities"))
                {
                    drawAbilityCategories();
                }
            }
            
            MonoScript newAbilityScript = EditorGUILayout.ObjectField(new GUIContent("Add New Ability", "Drag a script of a CogAbility here to add it to the manager"), null, typeof(MonoScript), false) as MonoScript;
            if (newAbilityScript)
            {
                checkForNewAbilities(newAbilityScript);
            }
        }
        EditorGUILayout.EndVertical();

        removeSelectedAbilities();
    }

    private void drawAbilityCategories()
    {
        EditorGUI.indentLevel++;
        foreach (eCogAbilityKeyword keyword in Enum.GetValues(typeof(eCogAbilityKeyword)))
        {
            drawAbilityCategory(keyword);
        }
        EditorGUI.indentLevel--;
    }

    private void drawAbilityCategory(eCogAbilityKeyword keyword)
    {

        //Should we even draw the category? We'll only draw categories that actually have abilities active in them.
        //We don't need to use the selection here since we're already making sure elsewhere not to print categories where multiple objects are selected and each has a different count.
        bool shouldDrawAbilitiesInThisCategory = (target as CogAbilityManager).CogAbilitiesFor(keyword).Count() > 0;

        if (shouldDrawAbilitiesInThisCategory)
        {
            if (ShowAbilityCategory[keyword] = EditorGUILayout.Foldout(ShowAbilityCategory[keyword], keyword.ToString()))
            {
                EditorGUI.indentLevel++;
                drawAbilitiesOfCategory(keyword);
                EditorGUI.indentLevel--;
            }
        }
    }

    private void drawAbilitiesOfCategory(eCogAbilityKeyword keyword)
    {
        //This a list of all selected object's CogAbilities lists that match the given keyword.
        List<List<CogAbility>> targetManagerAbilities = Selection.gameObjects
                                                                 .Select(selectedObject => selectedObject.GetComponent<CogAbilityManager>().CogAbilities
                                                                      .Where(ability => ability?.Keyword == keyword).ToList())
                                                                 .ToList();

        //If all the selected managers have the same number of items in this category.
        if (targetManagerAbilities.Max(abilities => abilities.Count) == targetManagerAbilities.Min(abilities => abilities.Count))
        {
            for (int i = 0; i < targetManagerAbilities[0].Count; ++i)
            {
                //Since we're observing the abilities of all selected cogs, we'll need to find the one refering to this current object.
                CogAbility targetObjectAbility = targetManagerAbilities.First(cogAbilityList => ShowAbility.ContainsKey(cogAbilityList[i]))
                                                                       .ToArray()[i];

                drawAbility(targetObjectAbility, 
                            targetManagerAbilities.Select(abilities => abilities[i]).ToArray());
            }
        }
        else
        {
            GUILayout.Label("Selected objects don't have the same abilities for this keyword");
        }
    }

    /// <summary>
    /// Draws the ability provided as a parameter.
    /// </summary>
    /// <param name="ability">The ability we want to draw.</param>
    /// <param name="abilityOnAllSelectedManagers">An array</param>
    /// <param name="o_RequestRemoval">Outputs an indication telling us whether we need to remove this ability from the manager.</param>
    private void drawAbility(CogAbility ability, CogAbility[] abilityOnAllSelectedManagers)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            if (ShowAbility[ability] = EditorGUILayout.Foldout(ShowAbility[ability], ability.GetType().GetDisplayName().Replace("Cog Ability", string.Empty)))
            {
                EditorGUI.indentLevel++;
                Editor.CreateEditor(abilityOnAllSelectedManagers).OnInspectorGUI();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove"))
                    {
                        m_AbilitiesToRemove.AddRange(abilityOnAllSelectedManagers);
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