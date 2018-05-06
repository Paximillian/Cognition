using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private eTargetType m_TargetType = eTargetType.All;

    /// <summary>
    /// The name of the function that serves as our Rpc.
    /// More on this in the comment on Rpc_TriggerVisuals().
    /// </summary>
    private string m_RpcTriggerVisualsName;
    /// <summary>
    /// A unique ID identifying this RPC amongst all other RPCs of the same signature on different instances of the same type on this object.
    /// We'll be using the index of this instance amongst the components of this object.
    /// More on this in the comment on Rpc_TriggerVisuals().
    /// </summary>
    private int m_RpcId;
    /// <summary>
    /// A unique identifier for the RPC we want to trigger.
    /// </summary>
    private int m_RpcTriggerVisualsHash;
    /// <summary>
    /// A NetworkWriter used to pass data to our RPC.
    /// </summary>
    private NetworkWriter m_RpcTriggerVisualsWriter = new NetworkWriter();

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
    public bool CanTrigger(eCogAbilityKeyword keyword, Cog invokingCog = null)
    {
        return keyword == Keyword &&
               (m_TargetType == eTargetType.All ||
                    (m_TargetType == eTargetType.Ally && TriggeringCog.HasSameOwnerAs(invokingCog) ||
                    (m_TargetType == eTargetType.Enemy && !TriggeringCog.HasSameOwnerAs(invokingCog)))) &&
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
        
        //Sends a custom RPC message.
        m_RpcTriggerVisualsWriter.StartMessage(MsgType.Rpc);
        m_RpcTriggerVisualsWriter.WritePackedUInt32((uint)m_RpcTriggerVisualsHash); //The unique identifier of the handler we registered in OnStartClient().
        m_RpcTriggerVisualsWriter.Write(netId); //The netId of the object that contains this handler.
        m_RpcTriggerVisualsWriter.Write(invokingCog?.netId ?? TriggeringCog.netId); //Additional data. In this case it's a NetworkInstanceId, because the original signature of the method
                                                                                    //we want to invoke is Rpc_TriggerVisuals(NetworkInstanceId invokerNetId)
        m_RpcTriggerVisualsWriter.FinishMessage();
        SendRPCInternal(m_RpcTriggerVisualsWriter, 0, m_RpcTriggerVisualsName);
    }
    #endregion EntryPoints

    #region PrivateMethods
    /// <summary>
    /// Sets up the visual effects RPC sender.
    /// More on this in the comment on Rpc_TriggerVisuals().
    /// </summary>
    private void setupRpcDispatchFields()
    {
        //We'll be using the index of this instance amongst the components of this object as the unique ID for this Rpc.
        m_RpcId = 0;
        foreach (CogAbility ability in GetComponents<CogAbility>())
        {
            if (ability == this) { break; }
            else { m_RpcId++; }
        }

        m_RpcTriggerVisualsName = nameof(Rpc_TriggerVisuals) + m_RpcId.ToString() + netId.ToString();
        m_RpcTriggerVisualsHash = m_RpcTriggerVisualsName.GetHashCode();
    }
    #endregion PrivateMethods

    #region NetworkMethods
    /// <summary>
    /// Sets up the visual effects RPC sender.
    /// More on this in the comment on Rpc_TriggerVisuals().
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

        setupRpcDispatchFields();
        RegisterRpcDelegate(GetType(), m_RpcTriggerVisualsHash, Rpc_TriggerVisuals);
    }

    /// <summary>
    /// Sets up the visual effects RPC sender.
    /// More on this in the comment on Rpc_TriggerVisuals().
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        setupRpcDispatchFields();
    }

    /// <summary>
    /// Activates any visual effects pertaining to this ability.
    /// </summary>
    //This is actually an RPC, but because of Unity bugs, we needed to implement this in another method.
    //Because we have multiple scripts on the same object that all inherit from the same script and all have the same RPC, Unity mixes up the RPCs and activates the ones on the wrong scripts of the object.
    //So instead of using an RPC, we're using the LLAPI's NetworkReader and NetworkWriter to send an RPC of our own.
    //The actual signature of this method should be: private void Rpc_TriggerVisuals(NetworkInstanceId invokerNetId)
    private void Rpc_TriggerVisuals(NetworkBehaviour obj, NetworkReader reader)
    {
        NetworkInstanceId invokerNetId = reader.ReadNetworkId();
        Cog invokingCog = ClientScene.FindLocalObject(invokerNetId).GetComponent<Cog>();

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