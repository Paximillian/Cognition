using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[AddComponentMenu("Object Pooling/Object Pool Manager")]
[Serializable]
public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField]
    private static ObjectPoolManager m_Instance = null;
    public static ObjectPoolManager Instance
    {
        get
        {
            return m_Instance;
        }
    }

    public List<GOListWrapper> ObjectsToPool = new List<GOListWrapper>();
    public List<string> ObjectPoolNames = new List<string>();
    public List<IntListWrapper> ObjectPoolStartAmounts = new List<IntListWrapper>();
    public List<int> SubListSizes = new List<int>();

    [SerializeField]
    private Dictionary<string, GameObjectPool> m_ObjectPoolDictionary = new Dictionary<string, GameObjectPool>();
    
    private void Awake()
    {
        m_Instance = this;
		
		if (GameObject.FindObjectsOfType(this.GetType()).Length > 1)
        {
            Debug.LogError("Can't have more than one Object Pool Manager in a scene.");
        }

        for(int i = 0; i < ObjectPoolStartAmounts.Count; ++i)
        {
            m_ObjectPoolDictionary.Add(ObjectPoolNames[i], null);
			m_ObjectPoolDictionary[ObjectPoolNames[i]] = new GameObjectPool();

            for (int j = 0; j < ObjectsToPool[i].InnerList.Count; ++j)
            {
				m_ObjectPoolDictionary[ObjectPoolNames[i]].AddSource(ObjectsToPool[i].InnerList[j], ObjectPoolStartAmounts[i].InnerList[j]);
			}
        }
    }

    /// <summary>
    /// Creates a new pool and adds it to our pool collection.
    /// </summary>
    /// <param name="i_PooledName">The name we'll use later to draw from this pool.</param>
    /// <param name="i_PoolSources">A list of sources this pool uses, meaning the possible objects that could be pulled and the initial amount to initialize each source with.</param>
    public void CreatePool(string i_PooledName, GameObject[] i_PoolSources, int[] i_PoolSourceCounts)
    {
        if (!DoesPoolExistFor(i_PooledName))
        {
            m_ObjectPoolDictionary.Add(i_PooledName, new GameObjectPool());

            for (int i = 0; i < i_PoolSources.Length; ++i)
            {
                m_ObjectPoolDictionary[i_PooledName].AddSource(i_PoolSources[i], i_PoolSourceCounts[i]);
            }
        }
    }

    /// <summary>
    /// Checks if a pool of the given tag name already exists.
    /// </summary>
    public bool DoesPoolExistFor(string i_PoolName)
    {
        return m_ObjectPoolDictionary.ContainsKey(i_PoolName);
    }

    public GameObjectPool GetPoolForObject(string i_ObjectPoolTag)
	{
		GameObjectPool returnedPool = null;
		
		try
		{
			returnedPool = m_ObjectPoolDictionary[i_ObjectPoolTag];
		}
		catch(KeyNotFoundException ex)
		{
			Debug.LogError(string.Format("The requested tag wasn't found in the dictionary.{0}Make sure that the tag is correct and that a pool by that name exists in your inspector.{0}{1}", Environment.NewLine, ex.Message));
		}

		return returnedPool;
    }

	public static GameObject PullObject(string i_ObjectPoolTag)
	{
		GameObject returnedObject = null;
		
		try
		{
			returnedObject = Instance.GetPoolForObject(i_ObjectPoolTag).PullObject();
		}
		catch(Exception ex)
		{
			if(ex is NullReferenceException)
			{
				Debug.LogError(string.Format("Couldn't find pool with tag: {0}{1}{2}", i_ObjectPoolTag, Environment.NewLine, ex.Message));
			}
			else if(ex is NoPoolSourcesException)
			{
				Debug.LogError(ex.Message);
			}
		}
		
		return returnedObject;
	}
}
