using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOtherRelayCogVisual : CogAbility, IGameMechanicAbility
{
        public override string Description
    {
        get
        {
            return "Gives you a boost of resources when it dies.";
        }
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        StartCoroutine(waitForOwnership(invokingCog));
    }

    private IEnumerator waitForOwnership(Cog invokingCog)
    {
        while ((invokingCog as PlayableCog).OwningPlayer == null)
        {
            yield return null;
        }

        //Yes we need this! Without it the cogs salute
        yield return null;
        yield return null;
        if (invokingCog.HasSameOwnerAs(TriggeringCog))
        {
            (invokingCog as RelayCog)?.ActivateRelayEffect(TriggeringCog);
        }
    }
}
