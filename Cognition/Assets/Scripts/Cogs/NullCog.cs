using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to signify that this is a helper nullCog. Should not be a network behaviour TODO: refactor so that it isn't
/// </summary>
public class NullCog : NeutralCog
{
    /// <summary>
    /// Does nothing on purpose because this type of cog doesn't conflict
    /// </summary>
    /// <param name="i_ConflictingCog"></param>
    override public void MakeConflicted(Cog i_ConflictingCog)
    {}
}