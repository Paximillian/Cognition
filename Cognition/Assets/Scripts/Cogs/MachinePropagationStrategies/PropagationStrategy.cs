using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the propogation strategy of a cog.
/// </summary>
[RequireComponent(typeof(Cog))]
public abstract class PropagationStrategy : MonoBehaviour
{
    #region Variables
    protected Cog Cog { get; private set; }
    #endregion Variables

    #region UnityMethods
    protected virtual void Awake()
    {
        Cog = GetComponent<Cog>();
    }
    #endregion UnityMethods

    #region AbstractMethods
    /// <summary>
    /// In order for the propagation to succeed, this rule must be met by the specific propagation strategy's rules.
    /// </summary>
    /// <param name="i_PotentialPropagationTarget">The cog we want to keep propagating to.</param>
    /// <returns>True if we should keep propagating or nah.</returns>
    protected abstract bool PropagationRule(Cog i_PotentialPropagationTarget);

    /// <summary>
    /// Called from a non-spinning cog as a callback to this cog trying to propagate to it.
    /// </summary>
    /// <param name="i_AskingCog">A non-spinning cog that this cog requested a propagation for in this recently.</param>
    /// <returns>True if the asking cog should start spinning, false if not.</returns>
    public abstract float CheckSpin(Cog i_AskingCog);
    #endregion AbstractMethods

    #region PublicMethods
    /// <summary>
    /// Asks this cog nicely to spin and tell its neighbours to also spin.
    /// </summary>
    /// <param name="i_RequestingCog">The cog that requested me to start spin.</param>
    /// <param name="i_Player">The player whose machine we're currently testing.</param>
    /// <returns>True if I'm now spinning or false if I'm not.</returns>
    public void InitializePropagation(NetworkPlayer i_Player, Cog i_RequestingCog, bool i_StopUnaffected = false)
    {
        //This is the start of a propogation first cog should check surroundings
        if (i_RequestingCog == null)
        {
            initiatePropagation(i_Player, i_StopUnaffected);
        }
        else
        {
            propagate(i_Player, i_RequestingCog, i_StopUnaffected);
        }
    }

    /// <summary>
    /// Checks if this cog should be conflicted.
    /// </summary>
    public bool CheckConflict(Cog i_AskingCog)
    {
        bool isConflicted = false;
        HashSet<Cog> conflictingNeighbors = new HashSet<Cog>();

        if (i_AskingCog.Spin != -Cog.Spin && Cog.Spin != 0f && i_AskingCog.Spin != 0)
        {
            i_AskingCog.MakeConflicted(Cog);
            Cog.MakeConflicted(i_AskingCog);
            isConflicted = true;
        }

        foreach (Cog neighbour in Cog.Neighbors)
        {
            foreach (Cog conflictingCog in Cog.IntersectingNeighborsFor(neighbour))
            {
                conflictingNeighbors.Add(conflictingCog);
            }
        }

        if (conflictingNeighbors.Count > 0)
        {
            Cog.MakeConflicted(i_AskingCog);
        }

        foreach (Cog conflictingCog in conflictingNeighbors)
        {
            conflictingCog.MakeConflicted(Cog);
            isConflicted = true;
        }

        return isConflicted;
    }
    #endregion PublicMethods

    #region PrivateMethods
    /// <summary>
    /// Passes on the spin request from this tile to its neighbours.
    /// </summary>
    /// <param name="i_Player"></param>
    /// <param name="i_RequestingCog"></param>
    /// <param name="i_StopUnaffected"></param>
    /// <returns></returns>
    private List<Tuple<Cog, Cog>> propagate(NetworkPlayer i_Player, Cog i_RequestingCog, bool i_StopUnaffected = false)
    {
        List<Tuple<Cog, Cog>> propogationPairs = new List<Tuple<Cog, Cog>>();
        i_Player.UpdatedCogs.Add(Cog);

        if (i_RequestingCog != null)
        {
            Cog.Rpc_UpdateSpin(Cog.Spin = i_RequestingCog.PropagationStrategy.CheckSpin(Cog));
            CheckConflict(i_RequestingCog);
        }

        foreach (Cog neighbor in Cog.HoldingTile.PopulatedNeighbors)
        {
            if (!i_Player.UpdatedCogs.Contains(neighbor))
            {
                if (PropagationRule(neighbor))
                {
                    //Add this pair of propogation target and requester to the BFS targets
                    propogationPairs.Add(new Tuple<Cog, Cog>(neighbor, Cog));
                    i_Player.UpdatedCogs.Add(neighbor);
                    neighbor.OccupyingPlayers.AddRange(Cog.OccupyingPlayers);
                }
            }
        }

        return propogationPairs;
    }

    /// <summary>
    /// Updates this cog's visual spinning by matching it to its neighbours spin.
    /// </summary>
    private void onCreateUpdateSpin()
    {
        foreach (Cog populatedNeighbor in Cog.HoldingTile.PopulatedNeighbors)
        {
            if (populatedNeighbor.Spin != 0f)
            {
                Cog.Rpc_UpdateSpin(Cog.Spin = populatedNeighbor.PropagationStrategy.CheckSpin(Cog));
                break;
            }
        }
    }

    /// <summary>
    /// Expands through the hex graph to update each cog on whether it should be spinning or not.
    /// </summary>
    /// <param name="i_StopUnaffected"></param>
    private void initiatePropagation(NetworkPlayer i_Player, bool i_StopUnaffected = false)
    {
        Cog playerBaseCog = i_Player.PlayerBaseCog;
        Tuple<Cog, Cog> current;
        Queue frontier = new Queue();
        i_Player.UpdatedCogs.Clear();

        onCreateUpdateSpin();

        //BFS initialization.
        frontier.Enqueue(new Tuple<Cog, Cog>(playerBaseCog, null));

        //BFS loop
        do
        {
            current = (Tuple<Cog, Cog>)frontier.Dequeue();

            List<Tuple<Cog, Cog>> nextLayer = current.Item1.PropagationStrategy.propagate(i_Player, current.Item2, i_StopUnaffected);

            foreach (Tuple<Cog, Cog> propogationPair in nextLayer)
            {
                frontier.Enqueue(propogationPair);
            }
        }
        while (frontier.Count > 0);

        //Stops the spinning of cogs that are not found by the BFS.
        if (i_StopUnaffected)
        {
            stopUnpropagatedCogs(i_Player);
            unoccupyNeutralCogs(i_Player);
        }

        i_Player.UpdatedCogs.Clear();
    }

    /// <summary>
    /// Removes the current player from the occupation of any neutral cogs that weren't reached by the propagation process.
    /// </summary>
    private void unoccupyNeutralCogs(NetworkPlayer i_Player)
    {
        Cog playerBaseCog = i_Player.PlayerBaseCog;
        IEnumerable<NeutralCog> stoppedCogs = NeutralCog.NeutralCogs.Except(i_Player.UpdatedCogs)
                                                                    .Cast<NeutralCog>();

        foreach (NeutralCog cogToStop in stoppedCogs)
        {
            if (cogToStop.OccupyingPlayers.Contains(i_Player))
            {
                cogToStop.OccupyingPlayers.Remove(i_Player);

                if (cogToStop.OccupyingPlayers.Count == 0)
                {
                    cogToStop.Rpc_UpdateSpin(0f);
                }
            }
        }
    }

    /// <summary>
    /// Stops the propagation of all owned cogs that weren't reached by the propagation process.
    /// </summary>
    private void stopUnpropagatedCogs(NetworkPlayer i_Player)
    {
        Cog playerBaseCog = i_Player.PlayerBaseCog;

        IEnumerable<Cog> StoppedCogs = i_Player.OwnedCogs.Except(i_Player.UpdatedCogs);

        foreach (Cog cogToStop in StoppedCogs)
        {
            cogToStop.Rpc_UpdateSpin(0f);
        }
    }
    #endregion PrivateMethods
}
