using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField]
    private HexTile m_TilePrefab;
    public HexTile TilePrefab { get { return m_TilePrefab; } }

    [SerializeField]
    private BaseCog m_ResourceCogPrefab;
    public BaseCog ResourceCogPrefab { get { return m_ResourceCogPrefab; } }

    [SerializeField]
    private BaseCog m_GoalCogPrefab;
    public BaseCog GoalCogPrefab { get { return m_GoalCogPrefab; } }

    [SerializeField]
    private BaseCog m_PlayerInitialCogPrefab;
    public BaseCog PlayerInitialCogPrefab { get { return m_PlayerInitialCogPrefab; } }
}