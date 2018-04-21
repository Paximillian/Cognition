using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CogEffectManager))]
public abstract class CogEffect : NetworkBehaviour
{
    #region Variables
    /// <summary>
    /// The name of the effect type will be used to categorize it's trigger time by making it into a keyword.
    /// </summary>
    public eCogEffectKeyword Keyword { get { return m_Keyword; } }
    [SerializeField]
    private eCogEffectKeyword m_Keyword;

    /// <summary>
    /// The cog that triggered the activation of this effect.
    /// </summary>
    protected Cog TriggeringCog { get; private set; }
    
    /// <summary>
    /// The description of this effect.
    /// </summary>
    protected abstract string Description { get; }
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
                    if (GetComponent<CogEffectManager>()?.CogEffects.Contains(this) ?? false) { }
                    else
                    {
                        DestroyImmediate(this, true);
                        UnityEditor.EditorUtility.DisplayDialog("you dun goofed", "You can't add a Cog Effect directly to an object, use the CogEffectManager script on the target objecto manage Cog Effects instead", "Cool, thx");
                    }
                }
            }
        };
    }
#endif
    #endregion UnityMethods

    #region EntryPoints
    /// <summary>
    /// Can this effect be triggered?
    /// </summary>
    /// <param name="keyword">The type of the requested effect. If this effect isn't of the given type then it won't trigger.</param>
    [Server]
    public bool CanTrigger(eCogEffectKeyword keyword)
    {
        return keyword == Keyword &&
               canTrigger();
    }

    /// <summary>
    /// Triggers this effect.
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
    /// Activates any visual effects pertaining to this effect.
    /// </summary>
    [ClientRpc]
    private void Rpc_TriggerVisuals(NetworkInstanceId invokerNetId)
    {
        Cog invokingCog = ClientScene.FindLocalObject(invokerNetId).GetComponent<Cog>();

        triggerVisuals(invokingCog);
    }
    #endregion NetworkMethods

    #region AbstractMethods
    /// <summary>
    /// Returns an indication of whether this effect can currently be triggered.
    /// </summary>
    [Server]
    protected abstract bool canTrigger();

    /// <summary>
    /// Activates the logic of this effect on the server.
    /// </summary>
    [Server]
    protected abstract void triggerLogic(Cog invokingCog);

    /// <summary>
    /// Activates the visual representation of this effect on the client.
    /// </summary>
    [Client]
    protected abstract void triggerVisuals(Cog invokingCog);
    #endregion AbstractMethods
}