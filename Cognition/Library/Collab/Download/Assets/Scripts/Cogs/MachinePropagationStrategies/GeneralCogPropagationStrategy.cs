using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GeneralCogPropagationStrategy : MonoBehaviour, IPropagationStrategy
{
    BaseCog Cog;
    public List<Tuple<BaseCog, BaseCog>> Propogate(NetworkPlayer i_Player, BaseCog i_RequestingCog, bool StopUnaffected = false)
    {
        float prevSpin = Cog.Spin;

        if (i_RequestingCog == null)
        {//This is the start of a propogation first cog should check surroundings
            OnCreateUpdateSpin();
            BFSUpdateDriven(StopUnaffected);
            return null;
        }
        else
        {

            Cog.Rpc_UpdateSpin(Cog.Spin = i_RequestingCog.PropagationStrategy.ShouldSpin(Cog));
            List<Tuple<BaseCog, BaseCog>> propogationPairs = new List<Tuple<BaseCog, BaseCog>>();
            i_Player?.updatedCogs.Add(Cog);

            if (prevSpin != Cog.Spin || StopUnaffected)
            {
                foreach (BaseCog neighbor in Cog.HolderTile.PopulatedNeighbors)
                {
                    if (!i_Player.updatedCogs.Contains(neighbor))
                    {
                        //neighbor.PropagationStrategy.Propogate(i_Player, Cog);
                        propogationPairs.Add(new Tuple<BaseCog, BaseCog>(neighbor, Cog)); //Add this pair of propogation target and requester to the BFS targets
                    }
                }
            }

            return propogationPairs;
        }
    }

    private void OnCreateUpdateSpin() {
        if (Cog.HolderTile.PopulatedNeighbors.Count() > 0) {
            Cog.Rpc_UpdateSpin(Cog.Spin = Cog.HolderTile.PopulatedNeighbors.First().PropagationStrategy.ShouldSpin(Cog));
        }
    }

    public float ShouldSpin(BaseCog i_AskingCog)
    {
        IEnumerable<BaseCog> conflictingNeighbors = Cog.IntersectingNeighborsFor(i_AskingCog);
        if (conflictingNeighbors.Count() > 0) {
            i_AskingCog.MakeConflicted();
            Cog.MakeConflicted();
            foreach (BaseCog conflictingcog in conflictingNeighbors) {
                conflictingcog.MakeConflicted();
            }
        }
        return - Cog.Spin;
    }

    void ActivateBFSUpdateDriven(bool delayedReaction = true, HexTile neighbor = null)
    {
        //StartCoroutine(BFSUpdateDriven());// Cog, delayedReaction, neighbor));
    }

    public void BFSUpdateDriven(bool StopUnaffected = false)//NetworkPlayer i_OriginPlayer, bool delayedReaction = true, HexTile i_HexTile = null)
    {
        //while (s_BFSsRunning > 0)
        //{
        //    yield return null;
        //}
        //s_BFSsRunning++;
        Cog.OwningPlayer.updatedCogs.Clear();
        Tuple<BaseCog, BaseCog> current;
        Queue frontier = new Queue();
        //List<BaseCog> visited = new List<BaseCog>();


        Cog.OwningPlayer.updatedCogs.Add(Cog);

        foreach (BaseCog neighbor in Cog.HolderTile.PopulatedNeighbors)
        {
            frontier.Enqueue(new Tuple<BaseCog, BaseCog>(neighbor, Cog));
        }
        //visited.Add(Cog);

        while (frontier.Count > 0) //BFS loop
        {
            current = (Tuple<BaseCog, BaseCog>)frontier.Dequeue();

            //visited.Add(current.Item1);
            Cog.OwningPlayer.updatedCogs.Add(Cog);

            List<Tuple<BaseCog, BaseCog>> nextLayer = current.Item1.PropagationStrategy.Propogate(Cog.OwningPlayer, current.Item2, StopUnaffected);

            foreach (Tuple<BaseCog, BaseCog> propogationPair in nextLayer)
            {
                frontier.Enqueue(propogationPair);
            }
        }

        if (StopUnaffected)
        {
            IEnumerable<BaseCog> StoppedCogs = Cog.OwningPlayer.OwnedCogs.Except(Cog.OwningPlayer.updatedCogs);
            //BaseCog[] sctest = StoppedCogs.ToArray();
            foreach (BaseCog cogToStop in StoppedCogs)
            {
                cogToStop.Rpc_UpdateSpin(0f);
            }
        }

        Cog.OwningPlayer.updatedCogs.Clear();
        //s_BFSsRunning--;
    }

    public void SetCog(BaseCog cog)
    {
        Cog = cog;
    }
}
