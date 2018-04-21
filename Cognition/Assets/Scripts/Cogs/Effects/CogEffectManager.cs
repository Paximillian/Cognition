using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// The cog effect manager gathers all cog effects on this cog and gives us one centralized point of access to the effect system.
/// </summary>
[RequireComponent(typeof(Cog))]
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
    [Server]
    public void TriggerEffects(eCogEffectKeyword keyword, Cog invokingCog = null)
    {
        foreach (CogEffect effect in CogEffects)
        {
            if (effect.CanTrigger(keyword))
            {
                effect.Trigger(invokingCog);
            }
        }
    }
    #endregion PublicMethods
}
