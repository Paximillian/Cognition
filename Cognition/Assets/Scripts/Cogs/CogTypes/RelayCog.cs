using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelayCog : PlayableCog
{
    [SerializeField]
    private GameObject m_relayEffectPrefab;

    [SerializeField]
    private Transform m_RelayEffectPivotPoint;

    private Dictionary<RelayCog, GameObject> m_connectedRelays = new Dictionary<RelayCog, GameObject>();

    protected override void ClientKillCog()
    {
        DeactivateRelayEffect(null);

        base.ClientKillCog();
    }

    public void ActivateRelayEffect(RelayCog i_activatingCog)
    {
        if (!m_connectedRelays.ContainsKey(i_activatingCog))
        {
            GameObject relayEffect = ObjectPoolManager.PullObject("RelayEffect");
            relayEffect.transform.SetParent(m_RelayEffectPivotPoint);
            relayEffect.transform.localPosition = Vector3.zero;

            m_connectedRelays.Add(i_activatingCog, relayEffect);

            m_connectedRelays[i_activatingCog].transform.LookAt(
                new Vector3(i_activatingCog.transform.position.x,
                m_connectedRelays[i_activatingCog].transform.position.y,
                i_activatingCog.transform.position.z));

            //If we're connecting this to an adjacent cog, we'll have to shorten the particles.
            if (i_activatingCog.HoldingTile.DistanceTo(this.HoldingTile) == 1)
            {
                foreach (ParticleSystem particles in m_connectedRelays[i_activatingCog].GetComponentsInChildren<ParticleSystem>())
                {
                    ParticleSystem.MainModule mainModule = particles.main;
                    mainModule.startLifetime = new ParticleSystem.MinMaxCurve(mainModule.startLifetime.constant / 2);

                   particles.transform.localPosition = new Vector3(particles.transform.localPosition.x,
                                                         particles.transform.localPosition.y,
                                                         particles.transform.localPosition.z / 2);
                }
            }
        }
    }

    public void DeactivateRelayEffect(RelayCog i_activatingCog)
    {
        if (i_activatingCog != null && m_connectedRelays.ContainsKey(i_activatingCog))
        {
            m_connectedRelays[i_activatingCog].transform.SetParent(ObjectPoolManager.Instance.transform);
            m_connectedRelays[i_activatingCog].SetActive(false);
            m_connectedRelays.Remove(i_activatingCog);
        }
        else if (i_activatingCog == null)
        {
            foreach (KeyValuePair<RelayCog, GameObject> connectedCog in m_connectedRelays)
            {
                connectedCog.Key.DeactivateRelayEffect(this);
                connectedCog.Value.transform.SetParent(ObjectPoolManager.Instance.transform);
                connectedCog.Value.SetActive(false);
            }

            m_connectedRelays.Clear();
        }
    }
}
