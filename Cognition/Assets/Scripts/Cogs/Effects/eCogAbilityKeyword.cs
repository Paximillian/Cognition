using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCogAbilityKeyword
{
    Bootup, //Triggers on build.
    Breakdown, //Triggers on destruction.
    Spin, //Triggers every frame.
    Connection, //Triggers when ANOTHER cog is built next to this one.
    Disconnection, //Triggers when ANOTHER adjacent cog is destroyed.
    Confliction, //Triggers when a conflict starts.
    Conflicted, //Triggers every frame while conflicted.
    Windup, //Triggered when the cog starts spinning from a stopped position, doesn't trigger on build.
    Winddown, //Triggered when the cog stops spinning from a spinning position, doesn't trigger on destruction.
}
