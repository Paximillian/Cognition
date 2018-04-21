using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haratzahta : CogEffect
{
    [SerializeField]
    private float m_Damage;

    protected override string Description
    {
        get
        {
            return "Damages any cogs built next to this one";
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
