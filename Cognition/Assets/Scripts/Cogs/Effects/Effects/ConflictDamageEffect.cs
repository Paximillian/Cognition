using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConflictDamageEffect : CogEffect
{

    /// <summary>
    /// How many resources are generated each time.
    /// </summary>
    [SerializeField]
    [Range(0, 10)]
    [Tooltip("How much damage is dealt per second of conflict?")]
    private float m_conflictDamage = 1f;

    protected override string Description
    {
        get
        {
            return "Causes damage to this cog while it is in a conflict.";
        }
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        TriggeringCog.DealDamage(m_conflictDamage * Time.deltaTime);
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        //Empty on purpose
        TriggeringCog.ShowConflictEffect(invokingCog.transform.position, true);
    }
}
