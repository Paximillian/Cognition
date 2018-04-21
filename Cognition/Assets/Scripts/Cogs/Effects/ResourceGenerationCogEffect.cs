using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceGenerationCogEffect : CooldownableCogEffect
{
    /// <summary>
    /// Amount of seconds of delay between the generation of resources for each player.
    /// </summary>
    [SerializeField]
    [Range(0.0001f, 1)]
    [Tooltip("Amount of seconds of delay between the generation of resources for each player.")]
    private float m_DelayBetweenPlayers = 0.01f;

    /// <summary>
    /// How many resources are generated each time.
    /// </summary>
    [SerializeField]
    [Range(1, 50)]
    [Tooltip("How many resources are generated each time.")]
    private int m_ResourcesPerGeneration = 10;

    protected override string Description
    {
        get
        {
            return "Generates resources for players powering up this cog";
        }
    }

    protected override void Awake()
    {
        base.Awake();

        Cooldown -= m_DelayBetweenPlayers * TriggeringCog.OccupyingPlayers.Count;
    }
    
    protected override void triggerLogic()
    {
        foreach (NetworkPlayer player in TriggeringCog.OccupyingPlayers)
        {
            player.Resources += m_ResourcesPerGeneration / TriggeringCog.OccupyingPlayers.Count;
        }
    }

    protected override void triggerVisuals()
    {
        StartCoroutine(showFloatingText());
    }

    private IEnumerator showFloatingText()
    {
        foreach (NetworkPlayer player in TriggeringCog.OccupyingPlayers)
        {
            FloatingMessage message = ObjectPoolManager.PullObject("ResourceGain").GetComponent<FloatingMessage>();
            message.SetInvokingPlayerId(player.PlayerId);
            message.transform.position = transform.position;
            message.Text.text = (m_ResourcesPerGeneration / TriggeringCog.OccupyingPlayers.Count).ToString();
            yield return new WaitForSeconds(m_DelayBetweenPlayers);
        }
    }
}