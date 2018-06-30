using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConflictDamageCogAbility : CooldownableCogAbility, IGameMechanicAbility
{
    private const float k_DamageTickCooldown = 0.1f;
    public static float DamageTickCooldown { get { return k_DamageTickCooldown; } }

    [SerializeField]
    [Range(0, 10)]
    [Tooltip("How much damage is dealt per tick of conflict?")]
    private double m_conflictDamage = 1f;
    public double ConflictDamage { get { return m_conflictDamage; } }

    protected new float Cooldown { get { return DamageTickCooldown; } }

    public override string Description
    {
        get
        {
            return base.Description + "Causes damage to this cog while it is in a conflict.";
        }
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        TriggeringCog.DealDamage(m_conflictDamage);
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        //Empty on purpose
    }
}
