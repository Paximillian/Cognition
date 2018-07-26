using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class AdaptableTest : MonoBehaviour
{
    private float m_Position = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        NetworkIdentity nId = GetComponent<NetworkIdentity>();

        if (nId && nId.isServer)
        {
            if (GUILayout.Button("Spawn 1"))
            {
                ObjectPoolManager.PullObject("Object1").transform.position = new Vector3(m_Position++, 0, 0);
            }
            if (GUILayout.Button("Spawn 2"))
            {
                NetworkObjectPoolManager.PullObject("Object2").transform.position = new Vector3(m_Position++, 0, 0);
            }
        }
    }
}
