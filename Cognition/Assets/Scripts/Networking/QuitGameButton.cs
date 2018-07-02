using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class QuitGameButton : MonoBehaviour
{
    public void Quit()
    {
        (NetworkManager.singleton as NetworkGameManager).QuitGame();
    }
}