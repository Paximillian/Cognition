using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalCog : BaseCog
{
    /// <summary>
    /// Amount of seconds between generation of progress.
    /// </summary>
    [SerializeField]
    [Range(0.01f, 1)]
    [Tooltip("Amount of seconds between generation of progress.")]
    private float m_GenerationInterval = 1f;

    /// <summary>
    /// Amount of seconds between generation of progress.
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("How much progress is acquired on each tick.")]
    private float m_ProgressPerGeneration = 0.01f;

    /// <summary>
    /// Amount of seconds of delay between the generation of resources for each player.
    /// </summary>
    [SerializeField]
    [Range(0.0001f, 1)]
    [Tooltip("Amount of seconds of delay between the generation of resources for each player.")]
    private float m_DelayBetweenPlayers = 0.01f;

    /// <summary>
    /// The progress towards the goal.
    /// Player 1 will drive this value up and player 2 will drive it down.
    /// When this reaches 1 or -1 respectively, the respective player wins.
    /// </summary>
    [Range(-1, 1)]
    [SyncVar(hook = "onCaptureProgressChanged")]
    private float m_Progress = 0;

    [ServerCallback]
    void Start()
    {
        StartCoroutine(generateResources());
    }

    [Client]
    private void onCaptureProgressChanged(float i_Progress)
    {
        m_Progress = i_Progress;

        //TODO: Replace with actual presentable method for goal progress.
        Debug.Log(m_Progress);
        if(m_Progress <= -1) { SceneManager.LoadScene("Player2Win"); }
        else if(m_Progress >= 1) { SceneManager.LoadScene("Player1Win"); }
    }

    [Server]
    private IEnumerator generateResources()
    {
        for (;;)
        {
            yield return new WaitForSeconds(m_GenerationInterval - m_DelayBetweenPlayers * OccupyingPlayers.Count);

            foreach (NetworkPlayer player in OccupyingPlayers)
            {
                int progressSign = player.PlayerId % 2 == 0 ? -1 : 1;
                m_Progress += m_ProgressPerGeneration / OccupyingPlayers.Count * progressSign;

                yield return new WaitForSeconds(m_DelayBetweenPlayers);

                Rpc_ShowFloatingText(player.PlayerId, (m_ProgressPerGeneration / OccupyingPlayers.Count).ToString());
            }
        }
    }

    [ClientRpc]
    private void Rpc_ShowFloatingText(int i_PlayerId, string i_Text)
    {
        FloatingMessage message = ObjectPoolManager.PullObject("GoalGain").GetComponent<FloatingMessage>();
        message.SetInvokingPlayerId(i_PlayerId);
        message.transform.position = transform.position;
        message.Text.text = i_Text;
    }
}