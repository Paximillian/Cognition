using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableConflictVisual : CogAbility, IGameMechanicAbility
{

    public override string Description
    {
        get
        {
            return base.Description + "Deactivates the visual representation of a conflict.";
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
        TriggeringCog.StopConflictEffect();
    }
}
