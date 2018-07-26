using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GameObjectPool 
{
    [SerializeField]
    private List<GameObject> m_GameObjectList = new List<GameObject>();

    [SerializeField]
    private List<GameObject> m_PoolObj = new List<GameObject>();
	private Dictionary<GameObject, int> m_RestockAmountForObject = new Dictionary<GameObject, int>();

	public GameObjectPool()
	{
	}

    public GameObjectPool(GameObject i_GameObjForPool, int i_Amount = 0)
    { 
		AddSource(i_GameObjForPool, i_Amount);
    }

	public void AddSource(GameObject i_GameObjForPool, int i_Amount = 1)
	{
		m_PoolObj.Add(i_GameObjForPool);
		m_RestockAmountForObject.Add(i_GameObjForPool, i_Amount);
		
		for (int i = 0; i < i_Amount; i++)
		{
			addToPool(i_GameObjForPool);
		}
	}

	private GameObject restockPool()
	{
		GameObject returnedObject = null;

		foreach(GameObject key in m_RestockAmountForObject.Keys)
		{
			returnedObject = addToPool(key);

			for(int i = 1; i < m_RestockAmountForObject[key]; ++i)
			{
				returnedObject = addToPool(key);
			}
		}

		return returnedObject;
	}

    private GameObject addToPool(GameObject i_PoolObject = null)
    {
		if(i_PoolObject == null)
		{
			i_PoolObject = m_PoolObj[Random.Range(0, m_PoolObj.Count)];
		}

        GameObject newGameobj = GameObject.Instantiate(i_PoolObject, new Vector3(-1337, -1337, -1337), Quaternion.identity, ObjectPoolManager.Instance.transform) as GameObject;
        newGameobj.SetActive(false);
        m_GameObjectList.Add(newGameobj);

        return newGameobj;
    }

    public GameObject PullObject()
	{
		if(m_PoolObj.Count == 0)
		{
			throw new NoPoolSourcesException();
		}

        GameObject objToPull = null;
		List<GameObject> pullableObjects = new List<GameObject>();

        foreach (GameObject obj in m_GameObjectList)
        {
            if (!obj.activeInHierarchy)
            {
				pullableObjects.Add(obj);
            }
        }

        if (pullableObjects.Count == 0)
        {
			objToPull = restockPool();
		}
		else
		{
			objToPull = pullableObjects[Random.Range(0, pullableObjects.Count)];
		}

		objToPull.SetActive(true);

        return objToPull; 
    }
}

