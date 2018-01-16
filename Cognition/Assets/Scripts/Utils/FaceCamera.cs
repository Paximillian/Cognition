using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class FaceCamera : MonoBehaviour
{
    // Update is called once per frame
    private void OnEnable()
    {
	transform.forward = Camera.main.transform.forward;
    }

}
