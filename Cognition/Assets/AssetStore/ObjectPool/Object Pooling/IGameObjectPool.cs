using UnityEngine;
using System.Collections;

public interface IGameObjectPool
{
    void AddSource(GameObject i_GameObjForPool, int i_Amount = 1);
    GameObject PullObject();
}
