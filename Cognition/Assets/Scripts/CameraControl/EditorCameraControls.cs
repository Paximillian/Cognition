using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The editor camera controls simply make an aggregation of all control types.
/// </summary>
public class EditorCameraControls : ICameraControls
{
    /// <summary>
    /// A list of all the control types we want to aggregate for editor testing.
    /// </summary>
    private ICameraControls[] m_Controllers = new ICameraControls[] 
    {
        new MobileCameraControls(),
        new PCCameraControls()
    };

    public Vector2 GetPanDelta()
    {
        return m_Controllers.Sum(controller => controller.GetPanDelta());
    }

    public Vector2 GetPosition()
    {
        return m_Controllers.Sum(controller => controller.GetPosition());
    }

    public Vector2 GetNormalizedPosition()
    {
        return m_Controllers.Sum(controller => controller.GetNormalizedPosition());
    }

    public float GetZoomDelta()
    {
        return m_Controllers.Sum(controller => controller.GetZoomDelta());
    }

    public void CancelGesture()
    {
        foreach (ICameraControls controller in m_Controllers)
        {
            controller.CancelGesture();
        }
    }

}
