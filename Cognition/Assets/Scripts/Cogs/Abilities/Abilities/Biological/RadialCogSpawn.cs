using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialCogSpawn : CogAbility
{
    /// <summary>
    /// How many resources are generated each time.
    /// </summary>
    [SerializeField]
    [Tooltip("The cog this effect should build")]
    private Cog m_spawnedCogPrefab;
    
    public override string Description
    {
        get
        {
            return base.Description + "Spawns the specified cog in every tile around the invoking cog.";
        }
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        invokingCog.HoldingTile.StartCoroutine(SpawnAfterDelay(invokingCog.HoldingTile, (invokingCog as PlayableCog)?.OwningPlayer));
    }

    private IEnumerator SpawnAfterDelay(HexTile targetTile, NetworkPlayer i_networkPlayer)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        foreach (HexTile tile in targetTile.Neighbors) {
            i_networkPlayer?.BuildCog(tile, m_spawnedCogPrefab);
        }
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        //Empty on purpose
    }
}
