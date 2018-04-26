using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkModeSetter : MonoBehaviour {

    private bool multiPlayerMode = false;
    [SerializeField]
    Text buttonText;

    public void ToggleMode() {

        multiPlayerMode = !multiPlayerMode;
        if (!multiPlayerMode)
        {
            NetworkManager.singleton.matchSize = 1;
            (NetworkManager.singleton as NetworkGameManager).DebugMode = true;
            buttonText.text = "SinglePlayer";
        }
        else {
            NetworkManager.singleton.matchSize = 2;
            (NetworkManager.singleton as NetworkGameManager).DebugMode = false;
            buttonText.text = "MultiPlayer";
        }
    }
}
