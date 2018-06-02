using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Grid system reference: http://keekerdc.com/2011/03/hexagon-grids-coordinate-systems-and-distance-calculations/
/// </summary>
[ExecuteInEditMode]
public class HexGrid : Singleton<HexGrid>
{
    [SerializeField]
    private HexTile m_TilePrefab;
    public HexTile TilePrefab { get { return m_TilePrefab; } }

    [SerializeField]
    private Cog m_ResourceCogPrefab;
    public Cog ResourceCogPrefab { get { return m_ResourceCogPrefab; } }

    [SerializeField]
    private Cog m_TurretCogPrefab;
    public Cog TurretCogPrefab { get { return m_TurretCogPrefab; } }

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

    /// <summary>
    /// A mapping between a coordinate to a tile.
    /// </summary>
    public Dictionary<Vector2Int, HexTile> Grid { get; private set; } = new Dictionary<Vector2Int, HexTile>();
    
    private void Start()
    {
        if (Grid.Count == 0)
        {
            setupGridCoordinates();
        }
    }

    private void setupGridCoordinates()
    {
        HexTile[] tiles = transform.GetComponentsInChildren<HexTile>(true);
        float topTileHeight = tiles.Max(tile => tile.transform.position.z);

        HexTile firstTile = tiles.Where(tile => Mathf.Approximately(tile.transform.position.z, topTileHeight))
                                 .OrderBy(tile => tile.transform.position.x)
                                 .First();
        
        mapTile(firstTile, Vector2Int.zero);
    }

    /// <summary>
    /// A recursive method that maps the given tile in the Grid's dictionary.
    /// </summary>
    private void mapTile(HexTile i_Tile, Vector2Int i_Coordinates)
    {
        if (i_Tile == null || Grid.ContainsValue(i_Tile))
        {
            return;
        }

        Grid.Add(i_Coordinates, i_Tile);
        i_Tile.Coordinates = i_Coordinates;

        mapTile(i_Tile.PositiveZNeighbour, i_Coordinates + Vector2Int.up);
        mapTile(i_Tile.NegativeZNeighbour, i_Coordinates - Vector2Int.up);
        mapTile(i_Tile.PositiveXNeighbour, i_Coordinates + Vector2Int.right);
        mapTile(i_Tile.NegativeXNeighbour, i_Coordinates - Vector2Int.right);
        mapTile(i_Tile.PositiveYNeighbour, i_Coordinates - Vector2Int.right + Vector2Int.up);
        mapTile(i_Tile.NegativeYNeighbour, i_Coordinates + Vector2Int.right - Vector2Int.up);
    }
}