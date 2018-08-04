using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathVisualEffectCogAbility : CogAbility, IGameMechanicAbility
{
    [SerializeField]
    private GameObject m_DeathEffectPrefab;

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        GameObject deathEffect = ObjectPoolManager.PullObject(m_DeathEffectPrefab.name);
        deathEffect.transform.position = TriggeringCog.transform.position;

        foreach (ParticleSystem particles in deathEffect.GetComponentsInChildren<ParticleSystem>())
        {
            particles.Play();
        }
    }
}