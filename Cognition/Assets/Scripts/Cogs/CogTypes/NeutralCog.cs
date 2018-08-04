using System;
using System.Collections.Generic;

public class NeutralCog : Cog
{
    public static HashSet<NeutralCog> NeutralCogs { get; private set; } = new HashSet<NeutralCog>();

    public override Func<Cog, bool> HasSameOwnerAs => ((i_AskingCog) => i_AskingCog == null ? false :
                                                                        (OccupyingPlayers.Contains((i_AskingCog as PlayableCog)?.OwningPlayer)));

    private void OnEnable()
    {
        NeutralCogs.Add(this);
    }

    private void OnDisable()
    {
        NeutralCogs.Remove(this);
    }
}
