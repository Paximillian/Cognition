using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NeutralCogPropagationStrategy : PropagationStrategy
{
    protected override bool PropagationRule(Cog i_PotentialPropagationTarget)
    {
        return true;
    }

    public override float CheckSpin(Cog i_AskingCog)
    {
        if (i_AskingCog.Spin == 0f)
        {
            return -Cog.Spin;
        }
        else
        {
            return i_AskingCog.Spin;
        }
    }
}
