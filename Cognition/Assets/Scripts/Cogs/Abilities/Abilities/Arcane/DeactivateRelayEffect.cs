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
        //if (invokingCog.HasSameOwnerAs(TriggeringCog)) {
        if (Keyword == eCogAbilityKeyword.Winddown)
        {
            //(invokingCog as RelayCog)?.DeactivateRelayEffect(null);
            (TriggeringCog as RelayCog)?.DeactivateRelayEffect(null);
        }
        else {
            (TriggeringCog as RelayCog)?.DeactivateRelayEffect(invokingCog);
        }
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        //Camera.main.backgroundColor = new Color(Random.value, Random.value, Random.value);
    }
}
