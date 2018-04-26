using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCogAbilityKeyword
{
    Bootup, //Triggers on build.
    Breakdown, //Triggers on destruction.
    Spin, //Triggers every frame.
    [DualFactionKeyword] Connection, //Triggers when ANOTHER cog is built next to this one.
    [DualFactionKeyword] Disconnection, //Triggers when ANOTHER adjacent cog is destroyed.
    [DualFactionKeyword] Confliction, //Triggers when a conflict starts.
    [DualFactionKeyword] Conflicted, //Triggers every frame while conflicted.
    Windup, //Triggered when the cog starts spinning from a stopped position, doesn't trigger on build.
    Winddown, //Triggered when the cog stops spinning from a spinning position, doesn't trigger on destruction.
}

public static class eCogAbilityKeywordExtensions
{
    /// <summary>
    /// Gets a hypertext string that represents the description intro of this keword.
    /// </summary>
    /// <param name="i_TargetType">The output text will be colored according to this target choice.</param>
    public static string GetDescriptionText(this eCogAbilityKeyword i_Keyword, CogAbility.eTargetType i_TargetType)
    {
        return $"<color=\"{i_TargetType.GetColor()}\"><b>{i_Keyword}: </b></color>";
    }

    public static string GetColor(this CogAbility.eTargetType i_TargetType)
    {
        switch (i_TargetType)
        {
            case CogAbility.eTargetType.Ally:
                return "green";
            case CogAbility.eTargetType.Enemy:
                return "red";
            case CogAbility.eTargetType.All:
            default:
                return "black";
        }
    }
}
