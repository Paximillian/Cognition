using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerResourceGenerationCogAbility : CooldownableCogAbility
{    
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
            return "Genreates resources for the player";
        }
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        NetworkPlayer owningPlayer = (TriggeringCog as PlayableCog).OwningPlayer;
        if (owningPlayer)
        {
            owningPlayer.Resources += m_ResourcesPerGeneration;
        }
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        NetworkPlayer owningPlayer = (TriggeringCog as PlayableCog).OwningPlayer;
        FloatingMessage message = ObjectPoolManager.PullObject("ResourceGain").GetComponent<FloatingMessage>();
        message.SetInvokingPlayerId(owningPlayer.PlayerId);
        message.transform.position = transform.position;
        message.Text.text = m_ResourcesPerGeneration.ToString();
    }
}
