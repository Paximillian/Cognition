using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlayableCog : Cog
{
    private const string k_UnitColorShaderProperty = "_UnitColor";

    private static Color s_LocalPlayerColor = Color.blue, s_EnemyPlayerColor = Color.red;

    /// <summary>
    /// The renderers used by this cog.
    /// </summary>
    private Renderer[] m_Renderers;

    /// <summary>
    /// All the friendly neighbours of this cog.
    /// </summary>
    public IEnumerable<Cog> FriendlyNeighbors => Neighbors.Where(neighbour => this.HasSameOwnerAs(neighbour));

    /// <summary>
    /// All the enemy neighbours of this cog.
    /// </summary>
    public IEnumerable<Cog> EnemyNeighbors => Neighbors.Where(neighbour => !this.HasSameOwnerAs(neighbour));

    /// <summary>
    /// Do this PlayableCog and the requesting cog have the same owner? Is false for non-playable cogs
    /// </summary>
    public override Func<Cog, bool> HasSameOwnerAs => ((i_AskingCog) => i_AskingCog == null ? false :
                                                                            (OwningPlayer.Equals((i_AskingCog as PlayableCog)?.OwningPlayer)));
    
    [SyncVar(hook = "onAssignedPlayerId")]
    private int m_OwningPlayerId;
    public int OwningPlayerId { get { return m_OwningPlayerId; } set { m_OwningPlayerId = value; } }
    private void onAssignedPlayerId(int i_OwningPlayerId)
    {
        m_OwningPlayerId = i_OwningPlayerId;

        foreach (Renderer renderer in m_Renderers)
        {
            if (renderer.material.HasProperty(k_UnitColorShaderProperty))
            {
                renderer.material.SetColor(k_UnitColorShaderProperty,
                                           m_OwningPlayerId == NetworkPlayer.LocalPlayer.PlayerId ? s_LocalPlayerColor : s_EnemyPlayerColor);
            }
        }
    }
    
    /// <summary>
    /// The player that placed this cog.
    /// </summary>
    public NetworkPlayer OwningPlayer
    {
        get { return m_OwningPlayer; }
        set
        {
            m_OwningPlayer = value;
            m_OwningPlayerNetId = value.netId;
        }
    }
    private NetworkPlayer m_OwningPlayer;
    [SyncVar(hook = "onAssignedPlayerNetId")]
    private NetworkInstanceId m_OwningPlayerNetId;

    private void onAssignedPlayerNetId(NetworkInstanceId i_NetId)
    {
        if (!isServer)
        {
            OwningPlayer = ClientScene.FindLocalObject(i_NetId).GetComponent<NetworkPlayer>();
        }
        
        gameObject.SetActive(true);
        StartCoroutine(delayedAlert());
    }

    /// <summary>
    /// Checks if an alert should be displayed to our local player if their opponent has built a unit off-screen.
    /// This is delayed for a short duration due to the position of the cog only being updated after this event occurs.
    /// TODO: We should find a way to reduce the delay to a minimum if this proves to be a UX problem.
    /// </summary>
    private IEnumerator delayedAlert()
    {
        yield return new WaitForSeconds(1);

        if (!OwningPlayer.Equals(NetworkPlayer.LocalPlayer))
        {
            if (!new Rect(0, 0, 1, 1).Contains(Camera.main.WorldToViewportPoint(transform.position)))
            {
                FloatingNotification buildNotification = ObjectPoolManager.PullObject("BuildNotification").transform.GetComponent<FloatingNotification>();
                buildNotification.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);
                buildNotification.SetTarget(this);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        m_Renderers = GetComponentsInChildren<Renderer>();
    }
}