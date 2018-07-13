using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class DemoFactionPicker : MonoBehaviour
{
    [SerializeField]
    private RadialMenuController m_ArcaneRadialMenu;
    [SerializeField]
    private Button m_ArcaneSelectionButton;

    [SerializeField]
    private RadialMenuController m_BioRadialMenu;
    [SerializeField]
    private Button m_BioSelectionButton;

    private void Awake()
    {
        Random rand = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);

        if (rand.Next() % 2 == 0)
        {
            ChooseArcaneFaction();
        }
        else
        {
            ChooseBioFaction();
        }
    }

    public void ChooseArcaneFaction()
    {
        StartCoroutine(chooseArcaneFaction());
    }

    private IEnumerator chooseArcaneFaction()
    {
        removeCurrentMenu();

        m_BioSelectionButton.targetGraphic.color = new Color(1, 1, 1, 1);
        m_BioSelectionButton.interactable = true;
        m_ArcaneSelectionButton.targetGraphic.color = new Color(1, 232f / 255f, 101f / 255f, 1);
        m_ArcaneSelectionButton.interactable = false;

        yield return new WaitForEndOfFrame();

        Instantiate(m_ArcaneRadialMenu);
    }

    public void ChooseBioFaction()
    {
        StartCoroutine(chooseBioFaction());
    }

    private IEnumerator chooseBioFaction()
    {
        removeCurrentMenu();

        m_BioSelectionButton.targetGraphic.color = new Color(1, 232f/255f, 101f/255f, 1);
        m_BioSelectionButton.interactable = false;
        m_ArcaneSelectionButton.targetGraphic.color = new Color(1, 1, 1, 1);
        m_ArcaneSelectionButton.interactable = true;

        yield return new WaitForEndOfFrame();

        Instantiate(m_BioRadialMenu);
    }

    private void removeCurrentMenu()
    {
        Destroy(GameObject.FindObjectOfType<RadialMenuController>()?.gameObject);
    }
}