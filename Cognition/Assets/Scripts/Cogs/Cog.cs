using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

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

    [SerializeField]
    private HexTile m_HoldingTile;
    public HexTile HoldingTile { get { return m_HoldingTile; } set { m_HoldingTile = value; } }

    [SerializeField]
    private float m_initialhp = 10f;
    public float InitialHP { get { return m_initialhp; } }

    [SerializeField]
    private float m_hp = 10f;
    public float HP { get { return m_hp; } }

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
    public HashSet<NetworkPlayer> OccupyingPlayers { get; set; } = new HashSet<NetworkPlayer>();
    #endregion Variables

    #region UnityMethods
    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        PropagationStrategy = GetComponent<PropagationStrategy>();
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

    #region PublicMethods
    public virtual void InvokeBattleCry() { }

    public virtual void InvokeDeathrattle() { }

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

        Animator.SetFloat("Spin", m_spin);
    }
    
    public void MakeConflicted(Cog i_ConflictingCog)
    {
        if (!m_Conflicted)
        {
            m_Conflicted = true;
            transform.localScale = Vector3.one + Vector3.right * 0.2f; //Conflict placeholder
            StartCoroutine(dealConflictDamage());
            i_ConflictingCog.PropagationStrategy.CheckConflict(this);
        }
    }
    
    public void StopConflicted()
    {
        m_Conflicted = false;
        transform.localScale = Vector3.one; //StopConflict placeholder
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
