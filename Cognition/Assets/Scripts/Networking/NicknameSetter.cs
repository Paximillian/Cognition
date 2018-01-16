using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NicknameSetter : MonoBehaviour {

    public void SetNickname(string i_Nickname)
    {
        PlayerPrefs.SetString("Nickname", String.IsNullOrWhiteSpace(i_Nickname) ? "Nope" : i_Nickname);
        PlayerPrefs.Save();
    }
}
