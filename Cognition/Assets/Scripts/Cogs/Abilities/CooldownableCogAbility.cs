﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class CooldownableCogAbility : CogAbility
{
    #region Variables
    /// <summary>
    /// When true, the next time this is ability is invoked, we'll let it pass through; Otherwise, it's still on cooldown.
    /// </summary>
    private bool m_ReadyToTrigger = true;

    /// <summary>
    /// Amount of seconds between triggers of the effect.
    /// </summary>
    [SerializeField]
    [Range(0, 20)]
    [Tooltip("Amount of seconds of cooldown between triggers.")]
    private float m_Cooldown = 5f;
    protected float Cooldown { get { return m_Cooldown; } set { m_Cooldown = value; } }
    #endregion Variables

    #region UnityMethods
    private void OnEnable()
    {
        m_ReadyToTrigger = true;
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