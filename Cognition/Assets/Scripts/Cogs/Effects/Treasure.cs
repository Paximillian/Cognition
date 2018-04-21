using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Treasure : CogEffect
{
    [SerializeField]
    private int m_ResourceAmount = 100;
    
    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic()
    {
        (TriggeringCog as PlayableCog).OwningPlayer.Resources += m_ResourceAmount;
    }

    protected override void triggerVisuals()
    {
        Camera.main.backgroundColor = new Color(Random.value, Random.value, Random.value);
    }
}
