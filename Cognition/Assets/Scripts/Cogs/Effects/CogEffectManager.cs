﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// The cog effect manager gathers all cog effects on this cog and gives us one centralized point of access to the effect system.
/// </summary>
public class CogEffectManager : NetworkBehaviour
{
    #region Variables
    /// <summary>
    /// The effects that can be triggered on this cog.
    /// </summary>
    public List<CogEffect> CogEffects { get; private set; }
    #endregion Variables

    #region UnityMethods
    private void OnEnable()
    {
        CogEffects = GetComponents<CogEffect>().ToList();
    }

    private void OnValidate()
    {
        CogEffects = GetComponents<CogEffect>().ToList();
    }
    #endregion UnityMethods

    #region PublicMethods
    /// <summary>
    /// Triggers all effects on this cog that match the given keyword.
    /// </summary>
    /// <param name="i_Keyword"></param>
    [Server]
    public void TriggerEffects(eCogEffectKeyword i_Keyword)
    {
        foreach (CogEffect effect in CogEffects)
        {
            if (effect.CanTrigger(i_Keyword))
            {
                effect.Trigger();
            }
        }
    }
    #endregion PublicMethods
}
