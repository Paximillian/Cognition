using UnityEngine;
using System.Collections;

public class OfflineTest : MonoBehaviour 
{
	private float m_Position = 0;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI()
	{
		if(GUILayout.Button("Spawn 1"))
		{
			ObjectPoolManager.PullObject("Object1").transform.position = new Vector3(m_Position++, 0, 0);
		}
		if(GUILayout.Button("Spawn 2"))
		{
			ObjectPoolManager.PullObject("Object2").transform.position = new Vector3(m_Position++, 0, 0);
		}
	}
}
