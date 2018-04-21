using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalCogEffect : CooldownableCogEffect
{
    #region Variables
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

    protected override string Description
    {
        get
        {
            return "Generates progress towards winning the game";
        }
    }
    #endregion Variables

    #region UnityMethods
    protected override void Awake()
    {
        base.Awake();

        Cooldown -= m_DelayBetweenPlayers * TriggeringCog.OccupyingPlayers.Count;
    }
    #endregion UnityMethods

    #region PrivateMethods
    [Client]
    private void onCaptureProgressChanged(float i_Progress)
    {
        m_Progress = i_Progress;

        //TODO: Replace with actual presentable method for goal progress.
        Debug.Log(m_Progress);
        TriggeringCog.Animator?.SetFloat("Win", Mathf.Abs(i_Progress));
        if (m_Progress <= -1) { SceneManager.LoadScene("Player2Win"); }
        else if(m_Progress >= 1) { SceneManager.LoadScene("Player1Win"); }
    }
    
    [Client]
    private IEnumerator displayDelayedEffects()
    {
        foreach (NetworkPlayer player in TriggeringCog.OccupyingPlayers)
        {
            FloatingMessage message = ObjectPoolManager.PullObject("GoalGain").GetComponent<FloatingMessage>();
            message.SetInvokingPlayerId(player.PlayerId);
            message.transform.position = transform.position;
            message.Text.text = (m_ProgressPerGeneration / TriggeringCog.OccupyingPlayers.Count).ToString();

            yield return new WaitForSeconds(m_DelayBetweenPlayers);
        }
    }
    #endregion PrivateMethods

    #region OverridenMethods
    protected override void triggerLogic()
    {
        foreach (NetworkPlayer player in TriggeringCog.OccupyingPlayers)
        {
            int progressSign = player.PlayerId % 2 == 0 ? -1 : 1;
            m_Progress += m_ProgressPerGeneration / TriggeringCog.OccupyingPlayers.Count * progressSign;
        }
    }

    protected override void triggerVisuals()
    {
        StartCoroutine(displayDelayedEffects());
    }
    #endregion OverridenMethods
}