using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGiftAccumulator : CooldownableCogAbility
{
    private ResourceGift m_ResourceGiftAbility;

    [SerializeField]
    [Tooltip("The amount of resources added to this cog every time this effect is triggered")]
    [Range(1, 10)]
    private int m_ResourceGainPerTick;

    [SerializeField]
    [Tooltip("The maximum amount of resources the cog can accumulate")]
    [Range(10, 500)]
    private int m_MaxResources;

    public override string Description
    {
        get
        {
            return base.Description + "Resources gained from death increase over time.";
        }
    }

    protected override void Awake()
    {
        base.Awake();

        m_ResourceGiftAbility = GetComponent<ResourceGift>();
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        int gain = (m_ResourceGiftAbility.ResourceAmount + m_ResourceGainPerTick) >= m_MaxResources ?
                            m_MaxResources - m_ResourceGiftAbility.ResourceAmount :
                            m_ResourceGainPerTick;
        
        m_ResourceGiftAbility.AddResources(gain);
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
    }
}
