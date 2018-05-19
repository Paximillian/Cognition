using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AreaDamageCogAbility : CooldownableCogAbility
{
    public NetworkPlayer CurrentOwner { get; set; }

    [Tooltip("What range of effect does this apply to?")]
    [SerializeField]
    [Range(1, 3)]
    private int m_DamageRange = 3;

    [Tooltip("How much damage does this deal?")]
    [SerializeField]
    private float m_Damage = 1;

    private ParticleSystem m_SplashRingParticle;

    protected override void Awake()
    {
        base.Awake();

        m_SplashRingParticle = GetComponentsInChildren<ParticleSystem>().FirstOrDefault(particles => particles.name.Equals("SplashRing"));
    }

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
        ParticleSystem.MainModule mainParticleModule = m_SplashRingParticle.main;
        mainParticleModule.startLifetime = 0.3f * m_DamageRange;
        mainParticleModule.startSize = 10 + (m_DamageRange - 1) * 7;

        TriggeringCog.Animator.SetTrigger("Drop");
    }
}
