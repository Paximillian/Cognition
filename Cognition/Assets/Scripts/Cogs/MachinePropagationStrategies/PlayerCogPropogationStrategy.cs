using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCogPropogationStrategy : GeneralCogPropagationStrategy
{

    BaseCog Cog;
    public new bool Propogate(NetworkPlayer i_Player, BaseCog i_RequestingCog)
    {
        base.Propogate(i_Player, i_RequestingCog);

        return true;
    }

    public new float ShouldSpin(BaseCog i_AskingCog)
    {
        return -Cog.HolderTile.Spin;
    }

}
