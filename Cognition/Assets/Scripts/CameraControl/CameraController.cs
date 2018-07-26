using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraModes { Regular, SingleFinger, Joystick, ModeSwitch}
public class CameraController : MonoBehaviour
{
    #region Variables
    private Camera m_mainCamera;
    private Vector3 m_CurrentBoundaryPositionTop;
    private Vector3 m_CurrentBoundaryPositionBottom;
    private Vector3 m_CurrentBoundaryPositionRight;
    private Vector3 m_CurrentBoundaryPositionLeft;
    private Vector3 m_ZoomOutOffsetDirection;

    [SerializeField] private float m_ZoomOffsetMultiplier;
    [SerializeField]
    private float m_ZoomSmoothFactor = 1f;

    private float m_previousFrameZoomDelta = 0f;

    private ICameraControls m_GestureHandler;

    /// <summary>
    /// Can we currently control the camera?
    /// </summary>
    private bool m_Enabled = false;
    #endregion Variables

    #region PublicMethods
    /// <summary>
    /// Enables the interactivity of this controller.
    /// </summary>
    public void Enable()
    {
        m_Enabled = true;
    }

    /// <summary>
    /// Disables the interactivity of this controller.
    /// </summary>
    public void Disable()
    {
        m_Enabled = false;
    }
    #endregion PublicMethods

    #region UnityMethods
    private void Awake()
    {
        m_mainCamera = Camera.main;

#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isRemoteConnected)
        {
            m_GestureHandler = new EditorCameraControls();
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
    }

    private void Update()
    {
        if (!m_Enabled)
        {
            return;
        }

        if (!RadialMenuController.Instance.IsActive)
        {
            m_CurrentBoundaryPositionTop = GetCorrectedCameraViewportPosition(HexGrid.Instance.CameraBoundaryTop.position);
            m_CurrentBoundaryPositionBottom = GetCorrectedCameraViewportPosition(HexGrid.Instance.CameraBoundaryBottom.position);
            m_CurrentBoundaryPositionRight = GetCorrectedCameraViewportPosition(HexGrid.Instance.CameraBoundaryRight.position);
            m_CurrentBoundaryPositionLeft = GetCorrectedCameraViewportPosition(HexGrid.Instance.CameraBoundaryLeft.position);

            checkZoom();
            checkPan();

            discardBoundaryPositionVectors();
        }
        else
        {
            m_GestureHandler.CancelGesture();
        }
    }

    private void discardBoundaryPositionVectors()
    {
        m_CurrentBoundaryPositionTop = Vector3.zero;
        m_CurrentBoundaryPositionBottom = Vector3.zero;
        m_CurrentBoundaryPositionRight = Vector3.zero;
        m_CurrentBoundaryPositionLeft = Vector3.zero;
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
        m_ZoomOutOffsetDirection = Vector3.zero;

        if (normalizedRectRange.Contains(m_CurrentBoundaryPositionTop)) { seenPoints++; m_ZoomOutOffsetDirection += Vector3.back; }
        if (normalizedRectRange.Contains(m_CurrentBoundaryPositionBottom)) { seenPoints++; m_ZoomOutOffsetDirection += Vector3.forward; }
        if (normalizedRectRange.Contains(m_CurrentBoundaryPositionRight)) { seenPoints++; m_ZoomOutOffsetDirection += Vector3.left; }
        if (normalizedRectRange.Contains(m_CurrentBoundaryPositionLeft)) { seenPoints++; m_ZoomOutOffsetDirection += Vector3.right; }

        return seenPoints;
    }

    /// <summary>
    /// Checks for any zooming input and executes it.
    /// </summary>
    private void checkZoom()
    {
        float zoomDelta = 
            Mathf.Approximately(m_previousFrameZoomDelta, 0f) ? 
            m_GestureHandler.GetZoomDelta() :
            Mathf.Clamp(m_GestureHandler.GetZoomDelta(), 
            m_previousFrameZoomDelta - m_ZoomSmoothFactor, 
            m_previousFrameZoomDelta + m_ZoomSmoothFactor);
        if (zoomDelta != 0)
        {
            Vector3 centerPosition = m_GestureHandler.GetPosition();
            
            int boundaryPointsInSight = seenBoundaryPointCount();
            //Zoom in
            if (zoomDelta > 0)
            {
                //if (boundaryPointsInSight > 0)
                if(transform.position.y > HexGrid.Instance.CameraZoomInBoundary.position.y)
                {
                    transform.position += transform.forward * zoomDelta;
                }
            }
            //Zoom out
            else if (zoomDelta < 0)
            {
                //if (boundaryPointsInSight < 4)
                if(transform.position.y < HexGrid.Instance.CameraZoomOutBoundary.position.y)
                {
                    transform.position += transform.forward * zoomDelta;
                    transform.position += (m_ZoomOutOffsetDirection * m_ZoomOffsetMultiplier);
                }
            }
        }
        m_previousFrameZoomDelta = zoomDelta;
        m_ZoomOutOffsetDirection = Vector3.zero;
    }

    /// <summary>
    /// Checks for any panning input and executes it.
    /// </summary>
    private void checkPan()
    {
        Vector3 panDelta = m_GestureHandler.GetPanDelta();
        panDelta = Vector3.ProjectOnPlane(panDelta, transform.up);

        if (panDelta.magnitude > 0) {
        }

        if (panDelta.x > 0)
        {
            if (m_CurrentBoundaryPositionRight.x > 1)
            {
                transform.position += Vector3.right * panDelta.x;
            }
        }
        else if (panDelta.x < 0)
        {
            if (m_CurrentBoundaryPositionLeft.x < 0)
            {
                transform.position += Vector3.right * panDelta.x;
            }
        }

        if (panDelta.y > 0)
        {
            if (m_CurrentBoundaryPositionTop.y > 1)
            {
                transform.position += Vector3.forward * panDelta.y;
            }
        }
        else if (panDelta.y < 0)
        {
            if (m_CurrentBoundaryPositionBottom.y < 0)
            {
                transform.position += Vector3.forward * panDelta.y;
            }
        }
    }

    private Vector3 GetCorrectedCameraViewportPosition(Vector3 cameraPlanePos)
    {
        return m_mainCamera.WorldToViewportPoint(CalculateProjectedCameraPlanePosition(cameraPlanePos, m_mainCamera));
    }

    // position = the world position of the entity to be tested
    private Vector3 CalculateProjectedCameraPlanePosition(Vector3 position, Camera camera)
    {
        //if the point is behind the camera then project it onto the camera plane
        Vector3 camNormal = camera.transform.forward;
        Vector3 vectorFromCam = position - camera.transform.position;
        float camNormDot = Vector3.Dot(camNormal, vectorFromCam.normalized);
        if (camNormDot <= 0f)
        {
            //we are beind the camera, project the position on the camera plane
            float camDot = Vector3.Dot(camNormal, vectorFromCam);
            Vector3 proj = (camNormal * camDot * 1.01f);   //small epsilon to keep the position infront of the camera
            position = camera.transform.position + (vectorFromCam - proj);
        }

        return position;
    }
    #endregion PrivateMethods
}
