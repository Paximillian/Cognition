using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PlayableCogPropagationStrategy : PropagationStrategy
{
    public new PlayableCog Cog { get { return base.Cog as PlayableCog; } }

    protected override bool PropagationRule(Cog i_PotentialPropagationTarget)
    {
        return i_PotentialPropagationTarget is NeutralCog ? true : 
            Cog.OwningPlayer == (i_PotentialPropagationTarget as PlayableCog)?.OwningPlayer;
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
