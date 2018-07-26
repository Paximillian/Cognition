using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class GOListWrapper
{
    public List<GameObject> InnerList = new List<GameObject>();

    public GOListWrapper()
    {
    }

    public GOListWrapper(int i_Capacity)
    {
        InnerList = new List<GameObject>(i_Capacity);
    }

    public GOListWrapper(IEnumerable<GameObject> i_CopyFrom)
    {
        InnerList = new List<GameObject>(i_CopyFrom);
    }
}
