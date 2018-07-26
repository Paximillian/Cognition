using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateRelayEffect : CogAbility, IGameMechanicAbility
{

    public override string Description
    {
        get
        {
            return "Turn off the relay cogs visual effect.";
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
        if (Keyword == eCogAbilityKeyword.Winddown)
        {
            (TriggeringCog as RelayCog)?.DeactivateRelayEffect(null);
        }
        else if (invokingCog is RelayCog)
        {
            (TriggeringCog as RelayCog)?.DeactivateRelayEffect(invokingCog as RelayCog);
        }
    }
}
