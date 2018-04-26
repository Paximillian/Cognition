using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConflictEngagedVisualEffect : CogAbility
{

    public override string Description
    {
        get
        {
            return "Activates the visual representation of a conflict.";
        }
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        //Empty on purpose
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        TriggeringCog.ShowConflictEffect(invokingCog.transform.position);
    }
}
