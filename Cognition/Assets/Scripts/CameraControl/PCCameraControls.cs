using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCCameraControls : ICameraControls
{
    private const float k_ScrollSensitivity = 1;

    private Vector3 m_LastMousePosition = Vector3.zero;

    public Vector2 GetPosition()
    {
        return Input.mousePosition;
    }

    public Vector2 GetPanDelta()
    {
        Vector2 delta = Vector2.zero;

        if (Input.GetMouseButton(2) && !Input.GetMouseButtonDown(2))
        {
            Vector3 currentPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Vector3.Distance(Camera.main.transform.position, Vector3.zero)));
            Vector3 lastPos = Camera.main.ScreenToWorldPoint(new Vector3(m_LastMousePosition.x, m_LastMousePosition.y, Vector3.Distance(Camera.main.transform.position, Vector3.zero)));
            
            delta = new Vector2((currentPos - lastPos).x, (currentPos - lastPos).z);
        }

        m_LastMousePosition = Input.mousePosition;

        return delta;
    }

    public float GetZoomDelta()
    {
        return Input.mouseScrollDelta.y * k_ScrollSensitivity;
    }
}