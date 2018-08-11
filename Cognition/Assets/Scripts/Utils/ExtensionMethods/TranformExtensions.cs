using UnityEngine;

public static class TransformExtensions
{
    public static T FindComponentInChildWithTag<T>(this Transform parent, string tag) where T : Component
    {
        Transform t = parent.transform;
        foreach (Transform tr in t)
        {
            if (tr.tag == tag)
            {
                return tr.GetComponent<T>();
            }
        }
        return null;
    }

    public static Transform FindChildWithTag(this Transform parent, string tag)
    {
        Transform t = parent.transform;
        foreach (Transform tr in t)
        {
            if (tr.tag == tag)
            {
                return tr;
            }
        }
        return null;
    }
}