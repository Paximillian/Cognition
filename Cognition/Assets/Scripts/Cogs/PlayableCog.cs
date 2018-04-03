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
    public Func<Cog, bool> HasSameOwnerAs => ((i_AskingCog) =>
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

        m_Renderer = GetComponentInChildren<Renderer>();
    }
}