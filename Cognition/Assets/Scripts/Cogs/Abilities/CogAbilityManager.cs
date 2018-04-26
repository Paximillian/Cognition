using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// The cog ability manager gathers all cog effects on this cog and gives us one centralized point of access to the effect system.
/// </summary>
[RequireComponent(typeof(Cog))]
public class CogAbilityManager : NetworkBehaviour
{
    #region Variables
    /// <summary>
    /// The abilities that can be triggered on this cog.
    /// </summary>
    public List<CogAbility> CogAbilities { get; private set; }

    /// <summary>
    /// The abilities of the given keyword that can be triggered on this cog.
    /// </summary>
    public Func<eCogAbilityKeyword, IEnumerable<CogAbility>> CogAbilitiesFor
    {
        get
        {
            return (keyword) => CogAbilities.Where(ability => ability?.Keyword == keyword);
        }
    }
    #endregion Variables

    #region UnityMethods
    private void OnEnable()
    {
        CogAbilities = GetComponents<CogAbility>().ToList();
    }

    private void OnValidate()
    {
        CogAbilities = GetComponents<CogAbility>().ToList();
    }
    #endregion UnityMethods

    #region PublicMethods
    /// <summary>
    /// Triggers all abilities on this cog that match the given keyword.
    /// </summary>
    [Server]
    public void TriggerAbilities(eCogAbilityKeyword keyword, Cog invokingCog = null)
    {
        foreach (CogAbility ability in CogAbilities)
        {
            if (ability.CanTrigger(keyword))
            {
                ability.Trigger(invokingCog);
            }
        }
    }
    #endregion PublicMethods
}
