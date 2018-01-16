using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceCog : BaseCog
{
    /// <summary>
    /// Amount of seconds between generation of resources.
    /// </summary>
    [SerializeField]
    [Range(1, 20)]
    [Tooltip("Amount of seconds between generation of resources.")]
    private float m_GenerationInterval = 5f;

    /// <summary>
    /// Amount of seconds of delay between the generation of resources for each player.
    /// </summary>
    [SerializeField]
    [Range(0.0001f, 1)]
    [Tooltip("Amount of seconds of delay between the generation of resources for each player.")]
    private float m_DelayBetweenPlayers = 0.01f;

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
            yield return new WaitForSeconds(m_GenerationInterval - m_DelayBetweenPlayers * OccupyingPlayers.Count);

            foreach(NetworkPlayer player in OccupyingPlayers)
            {
                player.Resources += m_ResourcesPerGeneration / OccupyingPlayers.Count;

		yield return new WaitForSeconds(m_DelayBetweenPlayers);
		
		Rpc_ShowFloatingText(player.PlayerId, (m_ResourcesPerGeneration / OccupyingPlayers.Count).ToString());
            }
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