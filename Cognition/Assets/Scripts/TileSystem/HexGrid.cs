using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : Singleton<HexGrid>
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

    [SerializeField]
    private Transform m_CameraBoundaryLeft;
    public Transform CameraBoundaryLeft { get { return m_CameraBoundaryLeft; } }

    [SerializeField]
    private Transform m_CameraBoundaryRight;
    public Transform CameraBoundaryRight { get { return m_CameraBoundaryRight; } }

    [SerializeField]
    private Transform m_CameraBoundaryTop;
    public Transform CameraBoundaryTop { get { return m_CameraBoundaryTop; } }

    [SerializeField]
    private Transform m_CameraBoundaryBottom;
    public Transform CameraBoundaryBottom { get { return m_CameraBoundaryBottom; } }
}