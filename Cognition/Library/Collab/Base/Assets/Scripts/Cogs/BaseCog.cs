using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public abstract class BaseCog : NetworkBehaviour
{
    /// <summary>
    /// The strategy this cog takes when dealing with propagating the spin of the machine.
    /// </summary>
    public IPropagationStrategy PropagationStrategy { get; private set; }

    /// <summary>
    /// The players that are powering this cog.
    /// This is different than the OwningPlayer property by that this also applies to neutral cogs like the resources or the goal cogs.
    /// </summary>
    public HashSet<NetworkPlayer> OccupyingPlayers { get; set; } = new HashSet<NetworkPlayer>();

    public Animator Animator { get; private set; }
    private Renderer m_Renderer;

    [SerializeField]
    private Material m_Player1Material, m_Player2Material;

    [SerializeField]
    [Range(1, 10)]
    private int m_Cost = 5;
    public int Cost { get { return m_Cost; } }

    /// <summary>
    /// The tile that this cog sits on.
    /// </summary>
    public HexTile HolderTile { get { return m_HolderTile; } set { m_HolderTile = value; } }
    [SerializeField]
    private HexTile m_HolderTile;

    /// <summary>
    /// The player that placed this cog.
    /// </summary>
    public NetworkPlayer OwningPlayer { get; set; }

    [SyncVar(hook = "onAssignedPlayerId")]
    private int m_OwningPlayerId;
    public int OwningPlayerId { get { return m_OwningPlayerId; } set { m_OwningPlayerId = value; } }
    private void onAssignedPlayerId(int i_OwningPlayerId)
    {
        m_OwningPlayerId = i_OwningPlayerId;

        m_Renderer.material = m_OwningPlayerId == 1 ? m_Player1Material : m_Player2Material;
    }
    [SerializeField]
    private float m_initialhp = 10f;

    public float Initial_HP { get { return m_initialhp; } }
    [SerializeField]
    private float m_hp = 10f;

    public float HP { get { return m_hp; } }

    protected bool m_IsActive
    {
        get
        {
            return HolderTile.Spin != 0f;
        }
    }

    public IEnumerable<BaseCog> Neighbors //Could cache this for performance sake if tile are static through out the game
    {
        get
        {
            return HolderTile.PopulatedNeighbors;
        }
    }

    public Func<BaseCog, IEnumerable<BaseCog>> IntersectingNeighborsFor
    {
        get
        {
            return ((cog) => cog.Neighbors.Intersect(Neighbors));
        }
    }

    [SerializeField]
    private Sprite m_Sprite;
    public Sprite Sprite { get { return m_Sprite; } private set { m_Sprite = value; } }

    [SerializeField]

    float m_spin = 0f;

    public float Spin { get { return m_spin; } }

    bool m_conflicted = false;

    public void DealDamage(float damage)
    {
        m_hp -= damage;
        if (m_hp <= 0f)
        {
            m_HolderTile.DestroyCog();

            transform.position = Vector3.one * -1337;
            Rpc_KillCog();
        }
    }

    [ClientRpc]
    private void Rpc_KillCog()
    {
        gameObject.SetActive(false);
    }

    public void resetCog()
    {
        m_hp = m_initialhp;
    }

    public virtual void InvokeDeathrattle()
    {
    }

    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        m_Renderer = GetComponentInChildren<Renderer>();
        PropagationStrategy = GetComponent<IPropagationStrategy>();
    }

    //Spin control

    [ClientRpc]
    private void Rpc_UpdateSpin(float i_SpinAmount)
    {
        if (!HolderTile.DrivingCog)
        {
            UpdateSpin(i_SpinAmount);
        }
    }

    [ClientRpc]
    private void Rpc_UpdateSpinInitial(float i_SpinAmount)
    {
        UpdateSpin(i_SpinAmount);
    }

    public void UpdateSpin(float spin)
    {
        //if (gameObject.name.Contains("4")) { Debug.Log(" 4 was spun " + spin); }
        StartCoroutine(updateSpin(spin));
    }

    private IEnumerator updateSpin(float spin)
    {
        m_spin = spin;

        Animator animator = null;
        do
        {
            yield return null;
            animator = Animator;//Will this not run forever on an empty tile?
        } while (animator == null);

        animator.SetFloat("Spin", m_spin);
    }
}
