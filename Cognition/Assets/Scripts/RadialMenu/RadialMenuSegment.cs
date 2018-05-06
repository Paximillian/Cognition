﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenuSegment : MonoBehaviour
{
    #region ExtraDefinitions
    [Flags]
    public enum eSegmentState
    {
        Available = 0,
        NotEnoughResources = 1,
        OutOfRange = 2,
        Highlighted = 4,
        Coooldown = 8
    }
    #endregion ExtraDefinitions

    #region Variables
    private eSegmentState m_CurrentState;
    public eSegmentState CurrentState
    {
        get { return m_CurrentState; }
        set
        {
            m_CurrentState = value;
            updateState(value);
        }
    }

    [SerializeField]
    private Image m_BackgroundImage;
    public Image BackgroundImage { get { return m_BackgroundImage; } }

    [SerializeField]
    private Image m_ItemIcon;
    public Image ItemIcon { get { return m_ItemIcon; } }

    [SerializeField]
    private Image m_DisabledBackgroundImage;
    public Image DisabledBackgroundImage { get { return m_DisabledBackgroundImage; } }

    [SerializeField]
    private TMP_Text m_CostText;
    public TMP_Text CostText { get { return m_CostText; } }
    #endregion Variables

    #region PublicMethods
    /// <summary>
    /// Updates the visual representation of this object based on the given state.
    /// </summary>
    private void updateState(eSegmentState i_NewState)
    {
        if (!BackgroundImage) { return;  }
        if (i_NewState.CheckState(eSegmentState.OutOfRange))
        {
            BackgroundImage.color = Color.black;
        }
        else if (i_NewState.CheckState(eSegmentState.NotEnoughResources))
        {
            BackgroundImage.color = Color.red;
        }
        else if (i_NewState.CheckState(eSegmentState.Highlighted) && !i_NewState.CheckState(eSegmentState.Coooldown))
        {
            BackgroundImage.color = Color.blue;
        }
        else if (i_NewState.CheckState(eSegmentState.Highlighted) && i_NewState.CheckState(eSegmentState.Coooldown))
        {
            BackgroundImage.color = Color.gray;
        }
        else if (i_NewState.CheckState(eSegmentState.Available) || i_NewState.CheckState(eSegmentState.Coooldown))
        {
            BackgroundImage.color = Color.white;
        }
    }
    #endregion PublicMethods
}
public static class SegmentStateExtensions
{
    /// <summary>
    /// Adds the given state to the current list of states.
    /// </summary>
    public static RadialMenuSegment.eSegmentState AddState(this RadialMenuSegment.eSegmentState i_CurrentState, RadialMenuSegment.eSegmentState i_NewState)
    {
        return i_CurrentState | i_NewState;
    }

    /// <summary>
    /// Removess the given state from the current list of states.
    /// </summary>
    public static RadialMenuSegment.eSegmentState RemoveState(this RadialMenuSegment.eSegmentState i_CurrentState, RadialMenuSegment.eSegmentState i_StateToRemove)
    {
        return i_CurrentState & (~i_StateToRemove);
    }

    /// <summary>
    /// Check if the given state is present in our current list of states.
    /// </summary>
    public static bool CheckState(this RadialMenuSegment.eSegmentState i_CurrentState, RadialMenuSegment.eSegmentState i_CheckedState)
    {
        return ((i_CurrentState & i_CheckedState) != 0) || 
            ((i_CurrentState == 0) && i_CheckedState == RadialMenuSegment.eSegmentState.Available);
    }
}