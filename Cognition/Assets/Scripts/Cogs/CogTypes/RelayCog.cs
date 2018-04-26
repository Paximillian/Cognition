using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelayCog : PlayableCog
{
    [SerializeField]
    private GameObject m_relayEffect;

    public void ActivateRelayEffect(Cog i_activatingCog) {
        m_relayEffect.transform.LookAt(
            new Vector3(i_activatingCog.transform.position.x, 
            m_relayEffect.transform.position.y, 
            i_activatingCog.transform.position.z));
        m_relayEffect.SetActive(true);
    }
}
