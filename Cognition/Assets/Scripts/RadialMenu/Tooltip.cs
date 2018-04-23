using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tooltip : Singleton<Tooltip>
{
    [SerializeField]
    private TMP_Text m_TooltipText;

    public void SetText(string text) => m_TooltipText.text = text;
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    protected override void Awake()
    {
        base.Awake();
        Hide();
    }
}