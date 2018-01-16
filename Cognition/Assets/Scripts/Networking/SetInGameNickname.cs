using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetInGameNickname : MonoBehaviour {

    public void SetInGameName(bool i_IsLocal)
    {
        if (i_IsLocal)
        {
            GetComponent<TextMesh>().text = NamesManager.Instance.LocalName;
        }
        else
        {
            GetComponent<TextMesh>().text = NamesManager.Instance.OpponentName;
        }
    }
}
