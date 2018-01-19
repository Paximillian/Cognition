﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(BaseCog))]
public class GeneralCogPropagationStrategy : PropagationStrategy
{
    public override List<Tuple<BaseCog, BaseCog>> Propogate(NetworkPlayer i_Player, BaseCog i_RequestingCog, bool i_StopUnaffected = false)
    {
        float prevSpin = Cog.Spin;

        //This is the start of a propogation first cog should check surroundings
        if (i_RequestingCog == null)
        {
            OnCreateUpdateSpin();
            BFSUpdateDriven(i_StopUnaffected);

            return null;
        }
        else
        {
            Cog.Rpc_UpdateSpin(Cog.Spin = i_RequestingCog.PropagationStrategy.ShouldSpin(Cog));
            List<Tuple<BaseCog, BaseCog>> propogationPairs = new List<Tuple<BaseCog, BaseCog>>();
            i_Player?.UpdatedCogs.Add(Cog);

            if (prevSpin != Cog.Spin || i_StopUnaffected)
            {
                foreach (BaseCog neighbor in Cog.HolderTile.PopulatedNeighbors)
                {
                    if (!i_Player.UpdatedCogs.Contains(neighbor))
                    {
                        //Add this pair of propogation target and requester to the BFS targets
                        propogationPairs.Add(new Tuple<BaseCog, BaseCog>(neighbor, Cog)); 
                    }
                }
            }

            return propogationPairs;
        }
    }

    private void OnCreateUpdateSpin()
    {
        foreach(BaseCog populatedNeighbor in Cog.HolderTile.PopulatedNeighbors)
        {
            if (populatedNeighbor.Spin != 0f) {
                Cog.Rpc_UpdateSpin(Cog.Spin = populatedNeighbor.PropagationStrategy.ShouldSpin(Cog));
                break;
            }
            //Cog.Rpc_UpdateSpin(Cog.Spin = Cog.HolderTile.PopulatedNeighbors.First().PropagationStrategy.ShouldSpin(Cog));
        }
    }

    public override float ShouldSpin(BaseCog i_AskingCog)
    {
        IEnumerable<BaseCog> conflictingNeighbors = Cog.IntersectingNeighborsFor(i_AskingCog);
        if (i_AskingCog.Spin == 0f)
        {
            if (conflictingNeighbors.Count() > 0 && Cog.Spin != 0f)
            {
                i_AskingCog.MakeConflicted();
                Cog.MakeConflicted();
                foreach (BaseCog conflictingcog in conflictingNeighbors)
                {
                    conflictingcog.MakeConflicted();
                }
            }

            return -Cog.Spin;
        }
        else {
            if (i_AskingCog.Spin != - Cog.Spin && Cog.Spin != 0f) {
                i_AskingCog.MakeConflicted();
                Cog.MakeConflicted();
            }
            return i_AskingCog.Spin;
        }
    }
}
