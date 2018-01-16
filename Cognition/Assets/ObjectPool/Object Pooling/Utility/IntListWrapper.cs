using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class IntListWrapper
{
    public List<int> InnerList = new List<int>();

    public IntListWrapper()
    {
    }

    public IntListWrapper(int i_Capacity)
    {
        InnerList = new List<int>(i_Capacity);
    }

    public IntListWrapper(IEnumerable<int> i_CopyFrom)
    {
        InnerList = new List<int>(i_CopyFrom);
    }
}
