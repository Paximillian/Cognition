using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCog : PlayableCog
{
    /// <summary>
    /// Amount of seconds between generation of resources.
    /// </summary>
    [SerializeField]
    [Range(1, 20)]
    [Tooltip("Amount of seconds between generation of resources.")]
    private float m_GenerationInterval = 5f;
    
    /// <summary>
    /// How many resources are generated each time.
    /// </summary>
    [SerializeField]
    [Range(1, 50)]
    [Tooltip("How many resources are generated each time.")]
    private int m_ResourcesPerGeneration = 10;

    [ServerCallback]
    private void Start()
    {
        StartCoroutine(generateResources());
    }

    [Server]
    private IEnumerator generateResources()
    {
        for (;;)
        {
            yield return new WaitForSeconds(m_GenerationInterval);

            OwningPlayer.Resources += m_ResourcesPerGeneration;
            Rpc_ShowFloatingText(OwningPlayer.PlayerId, m_ResourcesPerGeneration.ToString());
        }
    }

    [ClientRpc]
    private void Rpc_ShowFloatingText(int i_PlayerId, string i_Text)
    {
        FloatingMessage message = ObjectPoolManager.PullObject("ResourceGain").GetComponent<FloatingMessage>();
        message.SetInvokingPlayerId(i_PlayerId);
        message.transform.position = transform.position;
        message.Text.text = i_Text;
    }
}
