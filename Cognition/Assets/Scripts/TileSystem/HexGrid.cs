using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField]
    private HexTile m_TilePrefab;
    public HexTile TilePrefab { get { return m_TilePrefab; } }

    [SerializeField]
    private Cog m_ResourceCogPrefab;
    public Cog ResourceCogPrefab { get { return m_ResourceCogPrefab; } }

    [SerializeField]
    private Cog m_GoalCogPrefab;
    public Cog GoalCogPrefab { get { return m_GoalCogPrefab; } }

    [SerializeField]
    private Cog m_PlayerInitialCogPrefab;
    public Cog PlayerInitialCogPrefab { get { return m_PlayerInitialCogPrefab; } }
}