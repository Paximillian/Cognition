using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralCog : Cog
{
    public static HashSet<NeutralCog> NeutralCogs { get; private set; } = new HashSet<NeutralCog>();

    private void OnEnable()
    {
        NeutralCogs.Add(this);
    }

    private void OnDisable()
    {
        NeutralCogs.Remove(this);
    }
}
