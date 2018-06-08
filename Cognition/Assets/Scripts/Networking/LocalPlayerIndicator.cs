using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalPlayerIndicator : MonoBehaviour
{
    [SerializeField]
    private Color m_Player1Color, m_Player2Color;

    private void Update()
    {
        if (NetworkPlayer.LocalPlayer?.PlayerId != 0)
        {
            Image indicationIcon = GetComponentInChildren<Image>(true);

            indicationIcon.color = NetworkPlayer.LocalPlayer.PlayerId == 1 ? m_Player1Color : m_Player2Color;
            indicationIcon.gameObject.SetActive(true);

            Destroy(this);
        }
    }

}
