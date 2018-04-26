using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CountdownLabel : Singleton<CountdownLabel>
{
    private Text m_Label;
    public Text Label { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Label = GetComponent<Text>();
    }
}