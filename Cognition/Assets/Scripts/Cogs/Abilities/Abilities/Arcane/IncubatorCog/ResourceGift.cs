using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class ResourceGift : CogAbility
{
    [Tooltip("How many resources does this give if blown immediately")]
    [SerializeField]
    [Range(0, 100)]
    private int m_BaseResourceAmount = 1;

    [Tooltip("A text object representing how many resources this cog gives at the moment")]
    [SerializeField]
    private TMP_Text m_CurrentResourceCountDisplay;

    [SyncVar(hook = "onResourceAmountChanged")]
    private int m_ResourceAmount;
    public int ResourceAmount
    {
        get { return m_ResourceAmount; }
        private set { m_ResourceAmount = value; }
    }
    private void onResourceAmountChanged(int i_NewResourceCount)
    {
        m_ResourceAmount = i_NewResourceCount;
        m_CurrentResourceCountDisplay.text = m_ResourceAmount.ToString();
    }

    public override string Description
    {
        get
        {
            return base.Description + "Gives you a boost of resources when it dies.";
        }
    }

    private void OnEnable()
    {
        ResourceAmount = m_BaseResourceAmount;
    }

    public void AddResources(int i_ResourceGainPerTick)
    {
        ResourceAmount += i_ResourceGainPerTick;
    }

    protected override bool canTrigger()
    {
        return true;
    }

    protected override void triggerLogic(Cog invokingCog)
    {
        (TriggeringCog as PlayableCog).OwningPlayer.Resources += ResourceAmount;
    }

    protected override void triggerVisuals(Cog invokingCog)
    {
        //Camera.main.backgroundColor = new Color(Random.value, Random.value, Random.value);
    }
}
