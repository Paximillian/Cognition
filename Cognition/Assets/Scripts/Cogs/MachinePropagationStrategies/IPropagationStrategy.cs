using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the propogation strategy of a cog.
/// </summary>
public interface IPropagationStrategy
{

    /// <summary>
    /// Asks this cog nicely to spin and tell its neighbours to also spin.
    /// </summary>
    /// <param name="i_RequestingCog">The cog that requested me to start spin.</param>
    /// <param name="i_Player">The player whose machine we're currently testing.</param>
    /// <returns>True if I'm now spinning or false if I'm not.</returns>
    List<Tuple<BaseCog, BaseCog>> Propogate(NetworkPlayer i_Player, BaseCog i_RequestingCog, bool StopUnaffected = false);

    /// <summary>
    /// Called from a non-spinning cog as a callback to this cog trying to propagate to it.
    /// </summary>
    /// <param name="i_AskingCog">A non-spinning cog that this cog requested a propagation for in this recently.</param>
    /// <returns>True if the asking cog should start spinning, false if not.</returns>
    float ShouldSpin(BaseCog i_AskingCog);

    void SetCog(BaseCog cog);
}
