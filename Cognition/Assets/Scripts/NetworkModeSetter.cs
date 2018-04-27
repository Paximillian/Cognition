using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkModeSetter : MonoBehaviour {
    
    [SerializeField]
    Text buttonText;

    private void Start()
    {
        GetComponent<Button>().onClick?.Invoke();
    }

    public void ToggleSize()
    {
        buttonText.text = (NetworkManager.singleton.matchSize = (3 - NetworkManager.singleton.matchSize)).ToString();
    }

    public void ToggleDebug()
    {
        buttonText.text = ((NetworkManager.singleton as NetworkGameManager).DebugMode = !(NetworkManager.singleton as NetworkGameManager).DebugMode)
                            ? "Debug On"
                            : "Debug Off";
    }
}
