using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCog : PlayableCog
{
    protected override void Start()
    {
        base.Start();
        
        StartCoroutine(setMainCameraForLocalPlayer());

        if (isServer)
        {
            StartCoroutine(serverCheckConflictedCogsDestruction());
        }
    }

    private IEnumerator setMainCameraForLocalPlayer()
    {
        while (NetworkPlayer.LocalPlayer == null || OwningPlayer == null)
        {
            yield return null;
        }

        if (!NetworkPlayer.LocalPlayer.Equals(OwningPlayer))
        {
            Destroy(GetComponentInChildren<CinemachineVirtualCamera>(true).gameObject);
        }
        else
        {
            GetComponentInChildren<CinemachineVirtualCamera>(true).m_Priority = 10;
        }
    }

    /// <summary>
    /// This coroutine is run as a static coroutine on the cog class on the server to destroy cogs that were marked for the destruction at the end of the frame.
    /// This is a static coroutine in the sense that it only runs on the server's local player cog, which there is only 1 of in any game.
    /// </summary>
    [ServerCallback]
    private IEnumerator serverCheckConflictedCogsDestruction()
    {
        while (NetworkPlayer.LocalPlayer == null)
        {
            yield return null;
        }

        if (OwningPlayer.Equals(NetworkPlayer.LocalPlayer))
        {
            while (true)
            {
                //Creates a copy of the cog destruction list of this frame, so we can compare it with the one on the next frame, if they're identical, then the current conflict tick step has finished.
                //And we're ready to destroy the cogs without messing up the current step.
                List<Cog> cogsToDestroyOnLastFrame = CogsMarkedForDestruction.ToList();

                yield return new WaitForSeconds(ConflictDamageCogAbility.DamageTickCooldown);
                
                if (cogsToDestroyOnLastFrame.Count == CogsMarkedForDestruction.Count && CogsMarkedForDestruction.Count != 0)
                {
                    //We only destroy the cogs at the end of the propagation step, to avoid messing the current step by changing it while it's running.
                    foreach (Cog cogToDestroy in CogsMarkedForDestruction)
                    {
                        cogToDestroy.DestroyCog();
                    }

                    CogsMarkedForDestruction.Clear();
                }
            }
        }
    }
}
