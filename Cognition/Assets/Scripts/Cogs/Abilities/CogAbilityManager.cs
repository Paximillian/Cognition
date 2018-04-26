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
    private List<CogAbility> m_CogAbilities;
    public List<CogAbility> CogAbilities
    {
        get
        {
            if (m_CogAbilities == null)
            {
                m_CogAbilities = GetComponents<CogAbility>().ToList();
            }

            return m_CogAbilities;
        }
    }

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
        m_CogAbilities = null;
        var temp = CogAbilities;
    }

    private void OnValidate()
    {
        m_CogAbilities = null;
        var temp = CogAbilities;
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
            if (ability.CanTrigger(keyword, invokingCog))
            {
                ability.Trigger(invokingCog);
            }
        }
    }
    #endregion PublicMethods
}
