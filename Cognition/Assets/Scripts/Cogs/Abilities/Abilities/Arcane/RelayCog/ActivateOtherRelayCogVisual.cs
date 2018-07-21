using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActivateOtherRelayCogVisual : CogAbility, IGameMechanicAbility
{
        public override string Description
    {
        get
        {
            return "Displays the visual connection between 2 connected relay cogs.";
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
        yield return null;
        if (invokingCog.HasSameOwnerAs(TriggeringCog))
        {
            if (invokingCog.Equals(TriggeringCog))
            {
                foreach (Cog cog in (TriggeringCog.PropagationStrategy as RelayCogPropagationStrategy)
                                                  .Neighbors
                                                  .Where(cog => cog.HasSameOwnerAs(TriggeringCog)))
                {
                    if (cog.Spin != 0 && TriggeringCog.Spin != 0)
                    {
                        (cog as RelayCog)?.ActivateRelayEffect(TriggeringCog as RelayCog);
                    }
                }
            }
            else if (invokingCog?.Spin != 0 && TriggeringCog.Spin != 0)
            {
                (invokingCog as RelayCog)?.ActivateRelayEffect(TriggeringCog as RelayCog);
            }
        }
    }
}
