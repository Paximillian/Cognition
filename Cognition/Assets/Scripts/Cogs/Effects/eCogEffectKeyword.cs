using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCogEffectKeyword
{
    Bootup, //Triggers on build.
    Breakdown, //Triggers on destruction.
    Spin, //Triggers every frame.
    Connection, //Triggers when ANOTHER cog is built next to this one.
    Disconnection, //Triggers when ANOTHER adjacent cog is destroyed.
}
