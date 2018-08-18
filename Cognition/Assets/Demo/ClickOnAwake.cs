using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickOnAwake : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick?.Invoke();
    }
}