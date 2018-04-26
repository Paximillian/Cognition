using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RelayCogPropagationStrategy : PlayableCogPropagationStrategy
{
    [SerializeField]
    private int m_RelayRange = 2;

    /// <summary>
    /// Takes the neighbours of the cog as well as any other relay cogs in a larger range.
    /// </summary>
    public override IEnumerable<Cog> Neighbors => Cog.HoldingTile.PopulatedNeighborsInRadius(m_RelayRange)
                                                                      .Where(neighbour => neighbour.PropagationStrategy.GetType().Equals(typeof(RelayCogPropagationStrategy)))
                                                                      .Union(base.Neighbors);

    /// <summary>
    /// If we're being propelled by a relay cog, then we'll need to match our spin to theirs instead of reverse it.
    /// But if we're being propelled by a regular cog, then the normal calculation is fine.
    /// This is only relevant to even relay ranges, since odd relay ranges will want to keep the reversed spin.
    /// </summary>
    public override float CheckSpin(Cog i_AskingCog)
    {
        if (m_RelayRange % 2 == 0 &&
            i_AskingCog.PropagationStrategy.GetType().Equals(typeof(RelayCogPropagationStrategy)) &&
            !Cog.Neighbors.Contains(i_AskingCog))
        {
            if (i_AskingCog.Spin == 0f && (Cog.HasSameOwnerAs(i_AskingCog) || (i_AskingCog is NeutralCog)))
            {
                return Cog.Spin;
            }
            else
            {
                return i_AskingCog.Spin;
            }
        }

        return base.CheckSpin(i_AskingCog);
    }

    /// <summary>
    /// Relay cogs are conflicted a little differently.
    /// If we're spinning directly next to another cog, then the logic is the same, but since relayed propagation spins in the same direction, it doesn't cause a conflict for us.
    /// </summary>
    protected override bool CheckConfliction(Cog i_AskingCog)
    {
        if (m_RelayRange % 2 == 0 &&
            i_AskingCog.PropagationStrategy.GetType().Equals(typeof(RelayCogPropagationStrategy)) &&
            !Cog.Neighbors.Contains(i_AskingCog))
        {
            return i_AskingCog.Spin != Cog.Spin;
        }

        return base.CheckConfliction(i_AskingCog);
    }
}