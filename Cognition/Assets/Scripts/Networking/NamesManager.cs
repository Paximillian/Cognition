using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamesManager : MonoBehaviour {

    public static NamesManager Instance;

    public string LocalName { get; set; } = "Nope";
    public string OpponentName { get; set; } = "Nope";

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("Tried to make second instance of NamesManager, Game object destroyed.");
        }
    }
}
