using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeOverAbilityDamageCogAbility : CogAbility
{
    private AreaDamageCogAbility m_TurretAttackAbility;

    protected override void Awake()
    {
        base.Awake();
        m_TurretAttackAbility = GetComponent<AreaDamageCogAbility>();
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        m_TurretAttackAbility.CurrentOwner = (invokingCog as PlayableCog).OwningPlayer;
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
    }
}
