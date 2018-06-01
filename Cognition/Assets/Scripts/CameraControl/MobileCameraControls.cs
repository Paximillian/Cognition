using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MobileCameraControls : ICameraControls
{
    private const float k_JustSomeMagicNumberThing = 10;
    private const float k_ZoomSensitivity = 5;

    private Vector3 m_LastTouchPosition;
    private float m_LastZoomFingerDistance;

    public Vector2 GetPosition()
    {
        return new Vector2(Input.touches.Average(touch => touch.position.x),
                           Input.touches.Average(touch => touch.position.y));
    }

    public Vector2 GetPanDelta()
    {
        if (RadialMenuController.Instance.IsActive) { return Vector2.zero; }
        Vector2 delta = Vector2.zero;

        Vector3 currentPos = Input.touchCount > 0 ? Input.GetTouch(0).position
                                                  : Vector2.zero;

        if (Input.touchCount == 1 && Input.GetTouch(0).phase != TouchPhase.Began)
        {
            Vector3 currWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(currentPos.x, currentPos.y, Vector3.Distance(Camera.main.transform.position, Vector3.zero)));
            Vector3 lastPos = Camera.main.ScreenToWorldPoint(new Vector3(m_LastTouchPosition.x, m_LastTouchPosition.y, Vector3.Distance(Camera.main.transform.position, Vector3.zero)));

            delta = new Vector2((currWorldPos - lastPos).x, (currWorldPos - lastPos).z);
        }

        m_LastTouchPosition = currentPos;

        return -delta; 
    }

        public float GetZoomDelta()
    {
        float delta = 0;

        if (Input.touchCount == 2)
        {
            Vector3 touch1 = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, k_JustSomeMagicNumberThing));
            Vector3 touch2 = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(1).position.x, Input.GetTouch(1).position.y, k_JustSomeMagicNumberThing));
            
            float distance = Vector3.Distance(touch1, touch2);

            if (Input.GetTouch(1).phase != TouchPhase.Began)
            {
                delta = distance - m_LastZoomFingerDistance;
            }

            m_LastZoomFingerDistance = distance;
        }
        
        return delta * k_ZoomSensitivity;
    }
}