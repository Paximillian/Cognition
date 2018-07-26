using UnityEngine;
using System.Collections;

public interface IObjectPoolManager
{
    IGameObjectPool GetPoolForObject(string i_ObjectPoolTag);
    GameObject GetObject(string i_ObjectPoolTag);
}
