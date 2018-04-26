using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
[RequireComponent(typeof(CogAbilityManager))]
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
    public bool IsConflicted { get { return m_Conflicted; } }

    [SerializeField]
    [Range(1, 100)]
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
    [ReadOnly]
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
    public float Spin
    {
        get { return m_spin; }
        set
        {
            if (m_spin == 0 && value != 0 && isServer) { InvokeWindupAbilities(); }
            if (m_spin != 0 && value == 0 && isServer) { InvokeWinddownAbilities(); }

            m_spin = value;
        }
    }

    protected bool IsActive { get { return Spin != 0f; } }

    /// <summary>
    /// Has the build process of this cog completed?
    /// </summary>
    protected bool IsInitialized { get; private set; }
    
    /// <summary>
    /// The cogs that are placed in neighbouring hex cells to this one.
    /// </summary>
    public IEnumerable<Cog> Neighbors => HoldingTile.PopulatedNeighbors; 

    /// <summary>
    /// The cogs that are neighbours both for this tile as well as the given tile.
    /// </summary>
    public Func<Cog, IEnumerable<Cog>> IntersectingNeighborsFor => ((cog) => cog.Neighbors.Intersect(Neighbors));

    /// <summary>
    /// Do this cog and the requesting cog have the same owner? Is false for non-playable cogs
    /// </summary>
    public virtual Func<Cog, bool> HasSameOwnerAs => ((i_AskingCog) => false);

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
    /// The cog ability manager gathers all cog abilities on this cog and gives us one centralized point of access to the abilities system.
    /// </summary>
    protected CogAbilityManager CogAbilityManager
    {
        get
        {
            if (m_CogAbilityManager == null)
            {
                m_CogAbilityManager = GetComponent<CogAbilityManager>();
            }

            return m_CogAbilityManager;
        }
    }
    protected CogAbilityManager m_CogAbilityManager;

    /// <summary>
    /// A textual description of the functionality of this cog.
    /// </summary>
    [Tooltip("A textual description of the functionality of this cog.")]
    [SerializeField]
    private string m_Description;

    /// <summary>
    /// The text used to display information about this cog to the player.
    /// </summary>
    public string Description
    {
        get
        {
            return $"{m_Description}{Environment.NewLine}" +
                      string.Join($"{Environment.NewLine}{Environment.NewLine}", 
                                   CogAbilityManager.CogAbilities
                                                    .Where(ability => !(ability is IGameMechanicAbility))
                                                    .Select(ability => ability.Description));
        }
    }
    #endregion Variables

    #region UnityMethods
    protected virtual void Awake()
    {
        name += $"_{s_CogIndex++}";
        Animator = GetComponentInChildren<Animator>();
        PropagationStrategy = GetComponent<PropagationStrategy>();
        m_CogAbilityManager = GetComponent<CogAbilityManager>();
    }

    [ServerCallback]
    protected virtual void Update()
    {
        if (Spin != 0)
        {
            InvokeSpinAbilities();

            if (m_Conflicted)
            {
                InvokeConflictedAbilities();
            }
        }
    }
    #endregion UnityMethods

    #region UNETMethods
    [ClientRpc]
    private void Rpc_KillCog()
    {
        gameObject.SetActive(false);
    }

    [ClientRpc]
    private void Rpc_UpdateSpin(float i_SpinAmount)
    {
        UpdateSpin(i_SpinAmount);
    }
    #endregion UNETMethods

    #region CogEventHooks
    /// <summary>
    /// Bootup is the keyword indicating an ability that happens when a cog enters play.
    /// </summary>
    [Server]
    public void InvokeBootupAbilities()
    {
        CogAbilityManager.TriggerAbilities(eCogAbilityKeyword.Bootup);

        IsInitialized = true;
    }

    /// <summary>
    /// Breakdown is the keyword indicating an ability that happens when this cog is destroyed.
    /// </summary>
    [Server]
    public void InvokeBreakdownAbilities()
    {
        CogAbilityManager.TriggerAbilities(eCogAbilityKeyword.Breakdown);
    }

    /// <summary>
    /// Spin is the keyword indicating a constant ability that triggers every frame, the cog ability itself can determine the actual time between triggers by filtering invocations in its CanTrigger method
    /// </summary>
    [Server]
    public void InvokeSpinAbilities()
    {
        CogAbilityManager.TriggerAbilities(eCogAbilityKeyword.Spin);
    }

    /// <summary>
    /// Connection is the keyword indicating an ability that happens when ANOTHER cog is built adjacently to this one.
    /// Connection is NOT invoked on this cog when this cog itself is built next to another existing cog.
    /// </summary>
    [Server]
    public void InvokeConnectionAbilities(Cog connectedCog)
    {
        CogAbilityManager.TriggerAbilities(eCogAbilityKeyword.Connection, connectedCog);
    }

    /// <summary>
    /// Disconnection is the keyword indicating an ability that happens when ANOTHER neighbouring cog is destroyed.
    /// Disconnection is NOT invoked on this cog when this cog itself is destroyed next to another cog.
    /// </summary>
    [Server]
    public void InvokeDisconnectionAbilities(Cog disconnectedCog)
    {
        CogAbilityManager.TriggerAbilities(eCogAbilityKeyword.Disconnection, disconnectedCog);
    }

    /// <summary>
    /// Confliction is the keyword indicating an ability that happens when this cog has entered a conflict.
    /// </summary>
    [Server]
    public void InvokeConflictionAbilities()
    {
        CogAbilityManager.TriggerAbilities(eCogAbilityKeyword.Confliction);
    }

    /// <summary>
    /// Conflicted is the keyword indicating an ability that constantly happens when this cog is conflicted.
    /// </summary>
    [Server]
    private void InvokeConflictedAbilities()
    {
        CogAbilityManager.TriggerAbilities(eCogAbilityKeyword.Conflicted);
    }

    /// <summary>
    /// Windup is the keyword indicating an ability that happens when a cog starts spinning from a stopped state.
    /// This does NOT trigger on build.
    /// </summary>
    [Server]
    private void InvokeWindupAbilities()
    {
        if (IsInitialized)
        {
            CogAbilityManager.TriggerAbilities(eCogAbilityKeyword.Windup);
        }
    }

    /// <summary>
    /// Winddown is the keyword indicating an ability that happens when a cog stops spinning from a spinning state.
    /// This does NOT trigger on destruction.
    /// </summary>
    [Server]
    private void InvokeWinddownAbilities()
    {
        if (IsInitialized)
        {
            CogAbilityManager.TriggerAbilities(eCogAbilityKeyword.Winddown);
        }
    }
    #endregion CogEventHooks

    #region PublicMethods
    [Server]
    public void Heal(int healAmount)
    {
        m_hp = Mathf.Min(m_initialhp, m_hp + healAmount);
    }

    [Server]
    public void DealDamage(float damage)
    {
        m_hp -= damage;
        if (m_hp <= 0f)
        {
            StartCoroutine(DestroyCogAfterFrame());
            transform.position = Vector3.one * -1337;
        }
    }

    IEnumerator DestroyCogAfterFrame() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        DestroyCog();
    }

    public void DestroyCog() {
        m_HoldingTile.DestroyCog();
        Rpc_KillCog();
    }

    [Server]
    public void ResetCog()
    {
        IsInitialized = false;
        m_hp = m_initialhp;
        StopConflicted();

        UpdateSpin(Spin = 0f);
    }

    /// <summary>
    /// Requests the spin direction of the cog to be changed on the clients.
    /// </summary>
    /// <param name="i_SpinAmount"></param>
    [Server]
    public void RequestUpdateSpin(float i_SpinAmount)
    {
        Spin = i_SpinAmount;

        Rpc_UpdateSpin(i_SpinAmount);
    }

    /// <summary>
    /// Changes the spin direction of the cog on the client.
    /// </summary>
    [Client]
    public void UpdateSpin(float spin)
    {
        if (!isServer)
        {
            Spin = spin;
        }

        Animator?.SetFloat("Spin", Spin);
    }
    
    virtual public void MakeConflicted(Cog i_ConflictingCog)
    {
        if (!m_Conflicted)
        {
            m_Conflicted = true;

            //ShowConflictEffect(i_ConflictingCog.transform.position);
            //Rpc_ShowConflictEffect(i_ConflictingCog.transform.position);
            //
            //StartCoroutine(dealConflictDamage());
            i_ConflictingCog.PropagationStrategy.CheckConflict(this);

            InvokeConflictionAbilities();
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
