using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCogPropogationStrategy : PlayableCogPropagationStrategy
{
    public override float CheckSpin(Cog i_AskingCog)
    {
        return -Cog.Spin;
    }
}
