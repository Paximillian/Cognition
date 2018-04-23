using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Treasure : CogAbility
{
    [SerializeField]
    private int m_ResourceAmount = 100;

    public override string Description
    {
        get
        {
            return base.Description + "Test";
        }
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        (TriggeringCog as PlayableCog).OwningPlayer.Resources += m_ResourceAmount;
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        Camera.main.backgroundColor = new Color(Random.value, Random.value, Random.value);
    }
}
