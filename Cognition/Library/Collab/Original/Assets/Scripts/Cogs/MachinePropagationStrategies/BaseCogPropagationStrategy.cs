using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BaseCogPropagationStrategy : IPropagationStrategy
{
    BaseCog Cog;
    public bool Propogate(NetworkPlayer i_Player, BaseCog i_RequestingCog)
    {
        i_Player.updatedCogs.Add(Cog);
        i_RequestingCog.PropagationStrategy.ShouldSpin(Cog);
        foreach (BaseCog neighbor in Cog.HolderTile.PopulatedNeighbors)
        {
            if (!i_Player.updatedCogs.Contains(neighbor))
            {
                neighbor.PropagationStrategy.Propogate(i_Player, Cog);

            }
        }

        return true;
    }

    public float ShouldSpin(BaseCog i_AskingCog)
    {
        IEnumerable<BaseCog> conflictingNeighbors = Cog.IntersectingNeighborsFor(i_AskingCog);
        if (conflictingNeighbors.Count() > 0) {
            //i_AskingCog.conflict;
            //Cog.conflict;
            foreach (BaseCog conflictingcog in conflictingNeighbors) {
                //conflictingcog.conflict;
            }
        }
        return - Cog.HolderTile.Spin;
    }
}
