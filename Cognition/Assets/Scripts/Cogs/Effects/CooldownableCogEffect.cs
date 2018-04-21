using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class CooldownableCogEffect : CogEffect
{
    #region Variables
    /// <summary>
    /// When true, the next time this is invoked, we'll
    /// </summary>
    private bool m_ReadyToTrigger = false;

    /// <summary>
    /// Amount of seconds between generation of resources.
    /// </summary>
    [SerializeField]
    [Range(1, 20)]
    [Tooltip("Amount of seconds of cooldown between triggers.")]
    private float m_Cooldown = 5f;
    protected float Cooldown { get { return m_Cooldown; } set { m_Cooldown = value; } }
    #endregion Variables

    #region UnityMethods
    [ServerCallback]
    protected virtual void Start()
    {
        StartCoroutine(cooldownTicker());
    }
    #endregion UnityMethods

    #region PrivateMethods
    [Server]
    private IEnumerator cooldownTicker()
    {
        yield return new WaitForSeconds(m_Cooldown);

        m_ReadyToTrigger = true;
    }

    #endregion PrivateMethods

    #region OverridenMethods
    protected override bool canTrigger()
    {
        bool ready = m_ReadyToTrigger;
        m_ReadyToTrigger = false;

        if (ready)
        {
            StartCoroutine(cooldownTicker());
        }

        return ready;
    }
    #endregion OverridenMethods
}