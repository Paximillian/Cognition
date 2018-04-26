using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OopsPropagationStrategy : PropagationStrategy
{
    public override float CheckSpin(Cog i_AskingCog)
    {
        throw new NotImplementedException($"Oops! You're trying to move a cog without a proper propagation strategy attached: {Cog}");
    }

    protected override bool PropagationRule(Cog i_PotentialPropagationTarget)
    {
        throw new NotImplementedException($"Oops! You're trying to move a cog without a proper propagation strategy attached: {Cog}");
    }
}
