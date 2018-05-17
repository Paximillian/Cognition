using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDamageCogAbility : CooldownableCogAbility
{
    public NetworkPlayer CurrentOwner { get; set; }

    [Tooltip("What range of effect does this apply to?")]
    [SerializeField]
    private int m_DamageRange = 3;

    [Tooltip("How much damage does this deal?")]
    [SerializeField]
    private float m_Damage = 1;

    protected override void triggerLogic(Cog invokingCog)
    {
        foreach (Cog target in TriggeringCog.HoldingTile.PopulatedNeighborsInRadius(m_DamageRange))
        {
            if (target is PlayableCog && 
                (target as PlayableCog).OwningPlayer != CurrentOwner)
            {
                target.DealDamage(m_Damage);
            }
        }
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        TriggeringCog.Animator.SetTrigger("Drop");
    }
}