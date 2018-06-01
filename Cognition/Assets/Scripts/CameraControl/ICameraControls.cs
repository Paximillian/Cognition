using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICameraControls
{
    float GetZoomDelta();
    Vector2 GetPosition();
    Vector2 GetPanDelta();
}
