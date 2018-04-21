using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
[RequireComponent(typeof(CogEffectManager))]
public abstract class Cog : NetworkBehaviour
{
    #region Variables
    /// <summary>
    /// The index of the next cog to be created.
    /// </summary>
    private static int s_CogIndex = 0;

    public Animator Animator { get; private set; }

    /// <summary>
    /// Is the cog currently in conflict?
    /// </summary>
    private bool m_Conflicted = false;

    [SerializeField]
    [Range(1, 10)]
    private int m_Cost = 5;
    public int Cost { get { return m_Cost; } }

    /// <summary>
    /// Maximum build range away from other cogs beloning to the same player
    /// </summary>
    [Tooltip("Maximum build range away from other cogs beloning to the same player")]
    [SerializeField]
    [BuildRange(0,5)]
    private int m_buildRange = 1;
    public int BuildRange { get { return m_buildRange; } }

    [SerializeField]
    private HexTile m_HoldingTile;
    public HexTile HoldingTile { get { return m_HoldingTile; } set { m_HoldingTile = value; } }

    [SerializeField]
    private float m_initialhp = 10f;
    public float InitialHP { get { return m_initialhp; } }

    [SerializeField]
    private float m_hp = 10f;
    public float HP { get { return m_hp; } }

    [SerializeField] private ParticleSystem m_conflictParticles;

    [SerializeField]
    private Sprite m_Sprite;
    public Sprite Sprite { get { return m_Sprite; } private set { m_Sprite = value; } }

    [SerializeField]
    float m_spin = 0f;
    public float Spin { get { return m_spin; } set { m_spin = value; } }

    protected bool IsActive { get { return Spin != 0f; } }
    
    /// <summary>
    /// The cogs that are placed in neighbouring hex cells to this one.
    /// </summary>
    public List<Cog> Neighbors => HoldingTile.PopulatedNeighbors; 

    /// <summary>
    /// The cogs that are neighbours both for this tile as well as the given tile.
    /// </summary>
    public Func<Cog, List<Cog>> IntersectingNeighborsFor => ((cog) => cog.Neighbors.Intersect(Neighbors).ToList());

    /// <summary>
    /// Do this cog and the requesting cog have the same owner? Is false for non-playable cogs
    /// </summary>
    //public virtual Func<Cog, bool> HasSameOwner => ((i_AskingCog) => false);

    /// <summary>
    /// The strategy this cog takes when dealing with propagating the spin of the machine.
    /// </summary>
    public PropagationStrategy PropagationStrategy { get; private set; }

    /// <summary>
    /// The players that are powering this cog.
    /// This is different than the OwningPlayer property by that this also applies to neutral cogs like the resources or the goal cogs.
    /// </summary>
    public HashSet<NetworkPlayer> OccupyingPlayers
    {
        get
        {
            if (isServer) { StartCoroutine(sendOccupyingPlayersUpdate()); }
            return m_OccupyingPlayers;
        }
        set
        {
            if (isServer) { StartCoroutine(sendOccupyingPlayersUpdate()); }
            m_OccupyingPlayers = value;
        }
    }
    private HashSet<NetworkPlayer> m_OccupyingPlayers = new HashSet<NetworkPlayer>();

    /// <summary>
    /// Our OccupyingPlayers collection is an hashset, but we need to sync those values for various client-side calculations, which is not possible for this type.
    /// What we'll do is hook into the property and proc a syncing process that is emulating the process of a sync var syncing process.
    /// Once the list changes, we'll convert it to a json, which will be synced as a syncvar, and once it's received, we'll deserialize it back to an actual list.
    /// </summary>
    #region OccupyingPlayersSyncing
    [SyncVar(hook ="updateOccupyingPlayers")]
    private string m_OccupyingPlayersString;
    
    /// <summary>
    /// Converts the player list to a list of ids and converts it to a json that will be stored in the syncvar.
    /// </summary>
    private IEnumerator sendOccupyingPlayersUpdate()
    {
        yield return null;
        uint[] idList = m_OccupyingPlayers.Select(player => player.netId.Value).ToArray();
        m_OccupyingPlayersString = JsonConvert.SerializeObject(idList);
    }

    /// <summary>
    /// Called on clients once an update is sent, it converts the list back to a list of IDs.
    /// </summary>
    private void updateOccupyingPlayers(string idListString)
    {
        if (!isServer)
        {
            if (!idListString.Equals(m_OccupyingPlayersString))
            {
                m_OccupyingPlayersString = idListString;
                NetworkInstanceId[] idList = JsonConvert.DeserializeObject<uint[]>(idListString)
                                                        .Select(id => new NetworkInstanceId(id)).
                                                        ToArray();

                StartCoroutine(findPlayersForIds(idList));
            }
        }
    }

    /// <summary>
    /// Takes the id list and converts it back to a list of occupying plaers.
    /// </summary>
    /// <param name="idList"></param>
    /// <returns></returns>
    private IEnumerator findPlayersForIds(NetworkInstanceId[] idList)
    {
        do
        {
            m_OccupyingPlayers = new HashSet<NetworkPlayer>(idList.Select(id => ClientScene.FindLocalObject(id)?.GetComponent<NetworkPlayer>()));
            yield return new WaitForEndOfFrame();
        }
        while(m_OccupyingPlayers.Contains(null));
    }
    #endregion OccupyingPlayersSyncing

    /// <summary>
    /// The cog effect manager gathers all cog effects on this cog and gives us one centralized point of access to the effect system.
    /// </summary>
    protected CogEffectManager CogEffectManager { get; private set; }
    #endregion Variables

    #region UnityMethods
    protected virtual void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
        PropagationStrategy = GetComponent<PropagationStrategy>();
        CogEffectManager = GetComponent<CogEffectManager>();
    }

    [ServerCallback]
    protected virtual void Update()
    {
        InvokeSpinEffects();
    }
    #endregion UnityMethods

    #region UNETMethods
    [ClientRpc]
    private void Rpc_KillCog()
    {
        gameObject.SetActive(false);
    }

    [ClientRpc]
    public void Rpc_UpdateSpin(float i_SpinAmount)
    {
        UpdateSpin(i_SpinAmount);
    }
    #endregion UNETMethods

    #region CogEventHooks
    /// <summary>
    /// Bootup is the keyword indicating an effect that happens when a cog enters play.
    /// </summary>
    [Server]
    public void InvokeBootupEffects()
    {
        CogEffectManager.TriggerEffects(eCogEffectKeyword.Bootup);
    }

    /// <summary>
    /// Breakdown is the keyword indicating an effect that happens when this cog is destroyed.
    /// </summary>
    [Server]
    public void InvokeBreakdownEffects()
    {
        CogEffectManager.TriggerEffects(eCogEffectKeyword.Breakdown);
    }

    /// <summary>
    /// Spin is the keyword indicating a constant effect that triggers every frame, the cog effect itself can determine the actual time between triggers by filtering invocations in its CanTrigger method
    /// </summary>
    [Server]
    public void InvokeSpinEffects()
    {
        CogEffectManager.TriggerEffects(eCogEffectKeyword.Spin);
    }

    /// <summary>
    /// Connection is the keyword indicating an effect that happens when ANOTHER cog is built adjacently to this one.
    /// Connection is NOT invoked on this cog when this cog itself is built next to another existing cog.
    /// </summary>
    [Server]
    public void InvokeConnectionEffects(Cog connectedCog)
    {
        CogEffectManager.TriggerEffects(eCogEffectKeyword.Connection, connectedCog);
    }

    /// <summary>
    /// Disconnection is the keyword indicating an effect that happens when ANOTHER neighbouring cog is destroyed.
    /// Disconnection is NOT invoked on this cog when this cog itself is destroyed next to another cog.
    /// </summary>
    [Server]
    public void InvokeDisconnectionEffects(Cog disconnectedCog)
    {
        CogEffectManager.TriggerEffects(eCogEffectKeyword.Disconnection, disconnectedCog);
    }
    #endregion CogEventHooks

    #region PublicMethods
    public void DealDamage(float damage)
    {
        m_hp -= damage;
        if (m_hp <= 0f)
        {
            m_HoldingTile.DestroyCog();

            transform.position = Vector3.one * -1337;
            Rpc_KillCog();
        }
    }

    public void ResetCog()
    {
        name += $"_{s_CogIndex++}";
        m_hp = m_initialhp;
        StopConflicted();
        Rpc_UpdateSpin(m_spin = 0f);

    }
    
    public void UpdateSpin(float spin)
    {
        m_spin = spin;

        Animator?.SetFloat("Spin", m_spin);
    }
    
    virtual public void MakeConflicted(Cog i_ConflictingCog)
    {
        if (!m_Conflicted)
        {
            m_Conflicted = true;
            //transform.localScale = Vector3.one + Vector3.right * 0.2f; //Conflict placeholder
            ShowConflictEffect(i_ConflictingCog.transform.position);
            Rpc_ShowConflictEffect(i_ConflictingCog.transform.position);

            StartCoroutine(dealConflictDamage());
            i_ConflictingCog.PropagationStrategy.CheckConflict(this);
        }
    }

    [ClientRpc]
    public void Rpc_ShowConflictEffect(Vector3 conflictingCogPos)
    {
        ShowConflictEffect(conflictingCogPos);
    }

    public void ShowConflictEffect(Vector3 conflictingCogPos)
    {
        if (!m_conflictParticles) return;
        m_conflictParticles.gameObject.transform.position = (transform.position + conflictingCogPos) / 2f;
        m_conflictParticles.gameObject.SetActive(true);
        m_conflictParticles?.Play();
    }

    public void StopConflicted()
    {
        m_Conflicted = false;
        //transform.localScale = Vector3.one; //StopConflict placeholder
        StopConflictEffect();
        Rpc_StopConflictEffect();

    }

    [ClientRpc]
    public void Rpc_StopConflictEffect()
    {
        StopConflictEffect();
    }

    public void StopConflictEffect()
    {
        if (!m_conflictParticles) return;
        m_conflictParticles.Stop();
        m_conflictParticles.gameObject.SetActive(false);
    }
    #endregion PublicMethods

    #region PrivateMethods
    private IEnumerator dealConflictDamage(float damage = 1f)
    {
        while (m_Conflicted)
        {
            DealDamage(Time.deltaTime * damage);
            yield return new WaitForEndOfFrame();
        }

        StopConflicted();
    }
    #endregion PrivateMethods
}
