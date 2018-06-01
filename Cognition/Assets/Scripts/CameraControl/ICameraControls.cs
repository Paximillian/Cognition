using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICameraControls
{
    /// <summary>
    /// Gets the relative change in zoom input since the last frame.
    /// </summary>
    float GetZoomDelta();

    /// <summary>
    /// Gets the position of the pointer in screen space.
    /// </summary>
    Vector2 GetPosition();

    /// <summary>
    /// Gets the relative change in pointer position since the last frame.
    /// </summary>
    /// <returns></returns>
    Vector2 GetPanDelta();

    /// <summary>
    /// Cancels all gestures currently being checked for and resets the state of the tests.
    /// </summary>
    void CancelGesture();
}
