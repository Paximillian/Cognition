using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DetachOwnershipCogAbility : CogAbility
{
    private AreaDamageCogAbility m_TurretAttackAbility;

    protected override void Awake()
    {
        base.Awake();
        m_TurretAttackAbility = GetComponent<AreaDamageCogAbility>();
    }

    protected override bool canTrigger()
    {
        return true;
    }

    //Checks which player has the most influence over this cog now, and assigns ownership of this turret to that player.
    //If no players are occupying the cog, or if it's a tie, then ownership of this cog goes back to neutral.
    protected override void triggerLogic(Cog invokingCog)
    {
        IEnumerable connectedPlayers = TriggeringCog.Neighbors.Where(cog => cog != invokingCog)
                                                              .Select(cog => (cog as PlayableCog).OwningPlayer)
                                                              .Distinct();
        Dictionary<NetworkPlayer, int> ownerPowers = new Dictionary<NetworkPlayer, int>();

        foreach (NetworkPlayer player in connectedPlayers)
        {
            ownerPowers.Add(player, TriggeringCog.Neighbors.Count(cog => (cog as PlayableCog).OwningPlayer == player));
        }

        ownerPowers = ownerPowers.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (ownerPowers.Count > 0 &&
            (ownerPowers.Count < 2 ||
                ownerPowers.ElementAt(0).Value > ownerPowers.ElementAt(1).Value))
        {
            m_TurretAttackAbility.CurrentOwner = ownerPowers.ElementAt(0).Key;
        }
        else
        {
            m_TurretAttackAbility.CurrentOwner = null;
        }
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
    }
}