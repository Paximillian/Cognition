﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeighbourDamageCogAbility : CogAbility
{
    [SerializeField]
    private double m_Damage;

    public override string Description
    {
        get
        {
            return base.Description + "Damages any cogs built next to this one";
        }
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        invokingCog.DealDamage(m_Damage);
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        invokingCog.ShowConflictEffect(invokingCog.transform.position);
    }
}
