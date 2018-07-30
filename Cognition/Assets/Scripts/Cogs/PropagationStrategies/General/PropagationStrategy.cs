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
    private Cog m_cog;
    protected Cog Cog //NullExceptions were thrown on some cogs seemingly randomly when initialization was only in awake
    {
        get
        {
            if (m_cog == null)
            {
                m_cog = GetComponent<Cog>();
            }
            return m_cog;
        }
        private set { m_cog = value; }
    }

    /// <summary>
    /// A list of populated neighbours of this cog.
    /// </summary>
    public virtual IEnumerable<Cog> Neighbors => Cog.HoldingTile.PopulatedNeighbors;
        
    /// <summary>
    /// A list of cogs that are neighbours of both this cog and the given cog.
    /// </summary>
    protected virtual Func<Cog, IEnumerable<Cog>> IntersectingNeighborsFor => (cog) => Cog.IntersectingNeighborsFor(cog);
    #endregion Variables

    #region UnityMethods
    protected virtual void Awake()
    {
        if (m_cog == null)
        {
            Cog = GetComponent<Cog>();
        }
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
    /// <returns>True if after we finish propogation we should stop cogs we haven't visited.</returns>
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
        if (i_AskingCog is NullCog) { return false; }
        bool isConflicted = false;
        HashSet<Cog> conflictingNeighbors = new HashSet<Cog>();
        
        if (CheckConfliction(i_AskingCog) &&
            ((Cog.Spin != 0f && i_AskingCog.Spin != 0) ||
            ((i_AskingCog is PlayableCog) && (Cog is PlayableCog) && !(i_AskingCog as PlayableCog).HasSameOwnerAs(Cog))))
        {
            i_AskingCog.MakeConflicted(Cog);
            Cog.MakeConflicted(i_AskingCog);
            isConflicted = true;
        }

        foreach (Cog neighbour in Cog.Neighbors.Where(cog => !(cog is NullCog)))
        {
            foreach (Cog conflictingCog in IntersectingNeighborsFor(neighbour))
            {
                conflictingNeighbors.Add(conflictingCog);
            }
        }

        if (conflictingNeighbors.Count > 0)
        {
            Cog.MakeConflicted(conflictingNeighbors.ElementAt(0));
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
    /// Checks if we conflict with the given cog.
    /// Normally, a conflict is determined by the fact that the 2 cogs are spinning in the same direction.
    /// </summary>
    protected virtual bool CheckConfliction(Cog i_AskingCog)
    {
        return (i_AskingCog.Spin == 0 && Cog.Spin != 0) ||
               (i_AskingCog.Spin != 0 && Cog.Spin == 0) ||
               (Mathf.Sign(i_AskingCog.Spin) == Mathf.Sign(Cog.Spin) &&
                    (i_AskingCog.Spin != 0 && Cog.Spin != 0));
    }

    /// <summary>
    /// Passes on the spin request from this tile to its neighbours.
    /// </summary>
    /// <param name="i_Player"></param>
    /// <param name="i_RequestingCog"></param>
    /// <param name="i_StopUnaffected"></param>
    /// <returns></returns>
    private List<Tuple<Cog, Cog>> propagate(NetworkPlayer i_Player, Cog i_RequestingCog, bool i_StopUnaffected = false)
    {
        bool conflicted = false;
        List<Tuple<Cog, Cog>> propogationPairs = new List<Tuple<Cog, Cog>>();
        i_Player.UpdatedCogs.Add(Cog);

        if (i_RequestingCog != null)
        {
            Cog.RequestUpdateSpin(i_RequestingCog.PropagationStrategy.CheckSpin(Cog));
            conflicted = conflicted || CheckConflict(i_RequestingCog);
        }

        foreach (Cog neighbor in Neighbors)
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
                else {//Perform only a single step in this direction
                    neighbor.RequestUpdateSpin(Cog.PropagationStrategy.CheckSpin(neighbor));
                    conflicted = conflicted || CheckConflict(neighbor);
                }
            }
        }

        if (!conflicted) {
            Cog.StopConflicted();
        }

        return propogationPairs;
    }

    /// <summary>
    /// Updates this cog's visual spinning by matching it to its neighbours spin.
    /// </summary>
    private void onCreateUpdateSpin()
    {
        foreach (Cog populatedNeighbor in Neighbors)
        {
            if (populatedNeighbor.Spin != 0f && populatedNeighbor.HasSameOwnerAs(Cog))
            {
                Cog.RequestUpdateSpin(populatedNeighbor.PropagationStrategy.CheckSpin(Cog));
                //CheckConflict(neighbor);
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
        frontier.Enqueue(new Tuple<Cog, Cog>(Cog, null));

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
                //cogToStop.Rpc_RemoveOccupyingPlayer(i_Player.PlayerId);

                if (cogToStop.OccupyingPlayers.Count == 0)
                {
                    cogToStop.RequestUpdateSpin(0f);
                }
            }
        }
    }

    /// <summary>
    /// Stops the propagation of all owned cogs that weren't reached by the propagation process.
    /// </summary>
    private void stopUnpropagatedCogs(NetworkPlayer i_Player)
    {
        IEnumerable<Cog> StoppedCogs = i_Player.OwnedCogs.Except(i_Player.UpdatedCogs);

        //Stop all cogs needed
        foreach (Cog cogToStop in StoppedCogs)
        {
            cogToStop.RequestUpdateSpin(0f);
        }

        //Check if they should now conflict
        foreach (Cog cogToStop in StoppedCogs)
        {
            cogToStop.PropagationStrategy.CheckForConflicts();
        }
    }

    /// <summary>
    /// Checks for conflicts with surrounding neighbors. Used primarely to check for conflicts after stopping cogs because they aren't spun anymore
    /// </summary>
    public void CheckForConflicts()
    {
        foreach (Cog populatedNeighbor in Neighbors)
        {
            CheckConflict(populatedNeighbor);
        }
    }
    #endregion PrivateMethods
}
