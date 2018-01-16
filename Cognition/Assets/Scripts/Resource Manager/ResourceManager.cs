using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour {

    [SerializeField]
    private float m_BasicSalary = 100f;

    [SerializeField]
    private float m_TimeBetweenPaycheck = 10f;

    public float m_income { private set; get; }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
