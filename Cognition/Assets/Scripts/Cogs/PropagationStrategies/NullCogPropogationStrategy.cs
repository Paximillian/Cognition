using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullCogPropogationStrategy : PropagationStrategy
{
    [SerializeField]
    [Tooltip("Should this cog act normally or transfer rotation of itself directly to others?")]
    private bool directlyTransferRotation = false;

    protected override bool PropagationRule(Cog i_PotentialPropagationTarget)
    {
        return true;
    }

    public override float CheckSpin(Cog i_AskingCog)
    {
        if (i_AskingCog.Spin == 0f)
        {
            return directlyTransferRotation? Cog.Spin : - Cog.Spin;
        }
        else
        {
            return i_AskingCog.Spin;
        }
    }
}