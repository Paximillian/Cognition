using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CogAbilityManager))]
public abstract class CogAbility : NetworkBehaviour
{
    #region NestedDefinitions
    public enum eTargetType
    {
        All,
        Ally,
        Enemy
    }
    #endregion NestedDefinitions

    #region Variables
    /// <summary>
    /// The name of the ability type will be used to categorize it's trigger time by making it into a keyword.
    /// </summary>
    public eCogAbilityKeyword Keyword { get { return m_Keyword; } }
    [SerializeField]
    [Tooltip("The name of the ability type will be used to categorize it's trigger time by making it into a keyword.")]
    private eCogAbilityKeyword m_Keyword;

    /// <summary>
    /// Who does this ability affect? (This does NOT affect the ability's logic in any way, it's only used to display the keyword name for this ability in appropriate coloration.
    /// </summary>
    [SerializeField]
    [Tooltip("Who does this ability affect? (This does NOT affect the ability's logic in any way, it's only used to display the keyword name for this ability in appropriate coloration.")]
    [DualFactionConditionalHide(nameof(m_Keyword), true)]
    private eTargetType m_TargetType;

    /// <summary>
    /// The cog that triggered the activation of this ability.
    /// </summary>
    protected Cog TriggeringCog { get; private set; }
    
    /// <summary>
    /// The description of this ability.
    /// </summary>
    public virtual string Description
    {
        get
        {
            return Keyword.GetDescriptionText(m_TargetType);
        }
    }
    #endregion Variables

    #region UnityMethods
    protected virtual void Awake()
    {
        TriggeringCog = GetComponent<Cog>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            // ¯\_(ツ)_/¯
            if (this != null)
            {
                if ((hideFlags & HideFlags.HideInInspector) == 0)
                {
                    if (GetComponent<CogAbilityManager>()?.CogAbilities.Contains(this) ?? false) { }
                    else
                    {
                        DestroyImmediate(this, true);
                        UnityEditor.EditorUtility.DisplayDialog("you dun goofed", "You can't add a Cog Ability directly to an object, use the CogAbilityManager script on the target objecto manage Cog Abilities instead", "Cool, thx");
                    }
                }
            }
        };
    }
#endif
    #endregion UnityMethods

    #region EntryPoints
    /// <summary>
    /// Can this ability be triggered?
    /// </summary>
    /// <param name="keyword">The type of the requested ability. If this effect isn't of the given type then it won't trigger.</param>
    [Server]
    public bool CanTrigger(eCogAbilityKeyword keyword)
    {
        return keyword == Keyword &&
               canTrigger();
    }

    /// <summary>
    /// Triggers this ability.
    /// Can only be called from the server.
    /// </summary>
    [Server]
    public void Trigger(Cog invokingCog = null)
    {
        triggerLogic(invokingCog ?? TriggeringCog);

        Rpc_TriggerVisuals(invokingCog?.netId ?? TriggeringCog.netId);
    }
    #endregion EntryPoints

    #region NetworkMethods
    /// <summary>
    /// Activates any visual effects pertaining to this ability.
    /// </summary>
    [ClientRpc]
    private void Rpc_TriggerVisuals(NetworkInstanceId invokerNetId)
    {
        Cog invokingCog = ClientScene.FindLocalObject(invokerNetId).GetComponent<Cog>(); //Is this really necessary?

        triggerVisuals(invokingCog);
    }
    #endregion NetworkMethods

    #region AbstractMethods
    /// <summary>
    /// Returns an indication of whether this ability can currently be triggered.
    /// </summary>
    [Server]
    protected abstract bool canTrigger();

    /// <summary>
    /// Activates the logic of this ability on the server.
    /// </summary>
    [Server]
    protected abstract void triggerLogic(Cog invokingCog);

    /// <summary>
    /// Activates the visual representation of this ability on the client.
    /// </summary>
    [Client]
    protected abstract void triggerVisuals(Cog invokingCog);
    #endregion AbstractMethods
}