using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayableCog : Cog
{
    private Renderer m_Renderer;

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
    public NetworkPlayer OwningPlayer { get; set; }

    protected override void Awake()
    {
        base.Awake();

        m_Renderer = GetComponentInChildren<Renderer>();
    }
}