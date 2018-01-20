using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollectionExtensions
{
    public static void AddRange<T>(this HashSet<T> i_HashSet, IEnumerable<T> i_ItemsToAdd)
    {
        foreach (T item in i_ItemsToAdd)
        {
            i_HashSet.Add(item);
        }
    }
}