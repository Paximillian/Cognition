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
        //if (invokingCog.HasSameOwnerAs(TriggeringCog)) {
            (invokingCog as RelayCog)?.ActivateRelayEffect(TriggeringCog);
        //}
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        Camera.main.backgroundColor = new Color(Random.value, Random.value, Random.value);
    }
}
