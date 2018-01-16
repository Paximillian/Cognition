using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerIndicator : MonoBehaviour
{
    [SerializeField]
    private Material m_Player1Material, m_Player2Material;

    private void Update()
    {
        if (NetworkPlayer.LocalPlayer?.PlayerId != 0)
        {
            GetComponentInChildren<Renderer>().material = NetworkPlayer.LocalPlayer.PlayerId == 1 ? m_Player1Material : m_Player2Material;
            Destroy(this);
        }
    }

}
