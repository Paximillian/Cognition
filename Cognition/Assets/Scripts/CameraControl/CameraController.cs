using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraModes { Regular, SingleFinger, Joystick, ModeSwitch}
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    #region Variables
    private Camera m_Camera;
    private Camera m_mainCamera;

    private ICameraControls m_GestureHandler;

    public static float radialMenuDelay = 0f;
    #endregion Variables

    public void SetMode(int modeNum) {
        m_GestureHandler.SetMode((CameraModes)modeNum);
    }

    #region UnityMethods
    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
        m_mainCamera = Camera.main;

#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isRemoteConnected)
        {
            m_GestureHandler = new MobileCameraControls();
        }
        else
#endif
        if (Application.isMobilePlatform)
        {
            m_GestureHandler = new MobileCameraControls();
        }
        else
        {
            m_GestureHandler = new PCCameraControls();
        }

        m_GestureHandler.SetMode(CameraModes.Regular);
    }

    private void Update()
    {
        checkZoom();
        checkPan();
    }
    #endregion UnityMethods

    #region PrivateMethods
    /// <summary>
    /// How many camera boundary points are currently in view?
    /// </summary>
    private int seenBoundaryPointCount()
    {
        Rect normalizedRectRange = new Rect(0, 0, 1, 1);
        int seenPoints = 0;

        if (normalizedRectRange.Contains(m_mainCamera.WorldToViewportPoint(HexGrid.Instance.CameraBoundaryRight.position))) { seenPoints++; }
        if (normalizedRectRange.Contains(m_mainCamera.WorldToViewportPoint(HexGrid.Instance.CameraBoundaryLeft.position))) { seenPoints++; }
        if (normalizedRectRange.Contains(m_mainCamera.WorldToViewportPoint(HexGrid.Instance.CameraBoundaryTop.position))) { seenPoints++; }
        if (normalizedRectRange.Contains(m_mainCamera.WorldToViewportPoint(HexGrid.Instance.CameraBoundaryBottom.position))) { seenPoints++; }

        return seenPoints;
    }

    /// <summary>
    /// Checks for any zooming input and executes it.
    /// </summary>
    private void checkZoom()
    {
        float zoom = m_GestureHandler.GetZoomDelta();
        if (zoom != 0)
        {
            Vector3 centerPosition = m_GestureHandler.GetPosition();
            
            int boundaryPointsInSight = seenBoundaryPointCount();
            //Zoom in
            if (zoom > 0)
            {
                if (boundaryPointsInSight > 0)
                {
                    transform.position += transform.forward * zoom;
                }
            }
            //Zoom out
            else if (zoom < 0)
            {
                if (boundaryPointsInSight < 4)
                {
                    transform.position += transform.forward * zoom;
                }
            }
        }
    }

    /// <summary>
    /// Checks for any panning input and executes it.
    /// </summary>
    private void checkPan()
    {
        Vector3 panDelta = m_GestureHandler.GetPanDelta();
        panDelta = Vector3.ProjectOnPlane(panDelta, transform.up);

        if (panDelta.x > 0)
        {
            if (m_mainCamera.WorldToViewportPoint(HexGrid.Instance.CameraBoundaryRight.position).x > 1)
            {
                transform.position += Vector3.right * panDelta.x;
            }
        }
        else if (panDelta.x < 0)
        {
            if (m_mainCamera.WorldToViewportPoint(HexGrid.Instance.CameraBoundaryLeft.position).x < 0)
            {
                transform.position += Vector3.right * panDelta.x;
            }
        }

        if (panDelta.y > 0)
        {
            if (m_mainCamera.WorldToViewportPoint(HexGrid.Instance.CameraBoundaryTop.position).y > 1)
            {
                transform.position += Vector3.forward * panDelta.y;
            }
        }
        else if (panDelta.y < 0)
        {
            if (m_mainCamera.WorldToViewportPoint(HexGrid.Instance.CameraBoundaryBottom.position).y < 0)
            {
                transform.position += Vector3.forward * panDelta.y;
            }
        }
    }
    #endregion PrivateMethods
}
