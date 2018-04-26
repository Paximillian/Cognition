using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceGift : CogAbility
{
    [SerializeField]
    private int m_ResourceAmount = 100;

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
        (TriggeringCog as PlayableCog).OwningPlayer.Resources += m_ResourceAmount;
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        Camera.main.backgroundColor = new Color(Random.value, Random.value, Random.value);
    }
}
