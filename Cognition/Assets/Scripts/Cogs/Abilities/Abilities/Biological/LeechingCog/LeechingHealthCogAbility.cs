using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeechingHealthCogAbility : CogAbility
{
    [Tooltip("Amount of HP to heal from this cog")]
    [SerializeField]
    private int m_HealAmount;

    public override string Description
    {
        get
        {
            return base.Description + "Leeches health from nearby destroyed cogs";
        }
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        TriggeringCog.Heal(m_HealAmount);
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
    }
}