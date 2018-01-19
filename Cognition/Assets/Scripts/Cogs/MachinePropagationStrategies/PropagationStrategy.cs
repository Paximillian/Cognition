using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the propogation strategy of a cog.
/// </summary>
public abstract class PropagationStrategy : MonoBehaviour
{
    protected BaseCog Cog { get; private set; }

    protected virtual void Awake()
    {
        Cog = GetComponent<BaseCog>();
    }

    /// <summary>
    /// Asks this cog nicely to spin and tell its neighbours to also spin.
    /// </summary>
    /// <param name="i_RequestingCog">The cog that requested me to start spin.</param>
    /// <param name="i_Player">The player whose machine we're currently testing.</param>
    /// <returns>True if I'm now spinning or false if I'm not.</returns>
    public abstract List<Tuple<BaseCog, BaseCog>> Propogate(NetworkPlayer i_Player, BaseCog i_RequestingCog, bool StopUnaffected = false);

    /// <summary>
    /// Called from a non-spinning cog as a callback to this cog trying to propagate to it.
    /// </summary>
    /// <param name="i_AskingCog">A non-spinning cog that this cog requested a propagation for in this recently.</param>
    /// <returns>True if the asking cog should start spinning, false if not.</returns>
    public abstract float ShouldSpin(BaseCog i_AskingCog);
    
    /// <summary>
    /// Updates this cog's visual spinning by matching it to its neighbours spin.
    /// </summary>
    private void onCreateUpdateSpin()
    {
        if (Cog.HolderTile.PopulatedNeighbors.Count() > 0)
        {
            Cog.Rpc_UpdateSpin(Cog.Spin = Cog.HolderTile.PopulatedNeighbors.First().PropagationStrategy.ShouldSpin(Cog));
        }
    }

    /// <summary>
    /// Expands through the hex graph to update each cog on whether it should be spinning or not.
    /// </summary>
    /// <param name="i_StopUnaffected"></param>
    protected void BFSUpdateDriven(bool i_StopUnaffected = false)
    {
        Tuple<BaseCog, BaseCog> current;
        Queue frontier = new Queue();
        Cog.OwningPlayer.UpdatedCogs.Clear();
        Cog.OwningPlayer.UpdatedCogs.Add(Cog);

        onCreateUpdateSpin();

        //BFS initialization.
        foreach (BaseCog neighbor in Cog.HolderTile.PopulatedNeighbors)
        {
            frontier.Enqueue(new Tuple<BaseCog, BaseCog>(neighbor, Cog));
        }

        //BFS loop
        while (frontier.Count > 0)
        {
            current = (Tuple<BaseCog, BaseCog>)frontier.Dequeue();

            Cog.OwningPlayer.UpdatedCogs.Add(Cog);

            List<Tuple<BaseCog, BaseCog>> nextLayer = current.Item1.PropagationStrategy.Propogate(Cog.OwningPlayer, current.Item2, i_StopUnaffected);

            foreach (Tuple<BaseCog, BaseCog> propogationPair in nextLayer)
            {
                frontier.Enqueue(propogationPair);
            }
        }

        //Stops the spinning of cogs that are not found by the BFS.
        if (i_StopUnaffected)
        {
            IEnumerable<BaseCog> StoppedCogs = Cog.OwningPlayer.OwnedCogs.Except(Cog.OwningPlayer.UpdatedCogs);

            foreach (BaseCog cogToStop in StoppedCogs)
            {
                cogToStop.Rpc_UpdateSpin(0f);
            }
        }

        Cog.OwningPlayer.UpdatedCogs.Clear();
    }
}
