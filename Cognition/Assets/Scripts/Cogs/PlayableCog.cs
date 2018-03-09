using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayableCog : Cog
{
    private Renderer m_Renderer;

    /// <summary>
    /// Do this PlayableCog and the requesting cog have the same owner? Is false for non-playable cogs
    /// </summary>
    public Func<Cog, bool> HasSameOwner => ((i_AskingCog) =>
    (OwningPlayer.Equals((i_AskingCog as PlayableCog)?.OwningPlayer)));

    [SerializeField]
    private Material m_Player1Material, m_Player2Material;

    [SyncVar(hook = "onAssignedPlayerId")]
    private int m_OwningPlayerId;
    public int OwningPlayerId { get { return m_OwningPlayerId; } set { m_OwningPlayerId = value; } }
    private void onAssignedPlayerId(int i_OwningPlayerId)
    {
        m_OwningPlayerId = i_OwningPlayerId;
        m_Renderer.material = m_OwningPlayerId == 1 ? m_Player1Material : m_Player2Material;
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
    }

    protected override void Awake()
    {
        base.Awake();

        m_Renderer = GetComponentInChildren<Renderer>();
    }
}