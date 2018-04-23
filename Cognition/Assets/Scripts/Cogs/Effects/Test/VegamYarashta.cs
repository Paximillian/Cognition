using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegamYarashta : CogAbility
{
    protected override string Description
    {
        get
        {
            return "Leeches resources from nearby destroyed cogs";
        }
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        (TriggeringCog as PlayableCog).OwningPlayer.Resources += invokingCog.Cost;
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
    }
}