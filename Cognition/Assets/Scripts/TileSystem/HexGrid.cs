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

    /// <summary>
    /// A mapping between a tile to a coordinate.
    /// </summary>
    private Dictionary<HexTile, Vector2Int> m_ReverseGrid = new Dictionary<HexTile, Vector2Int>();

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
        m_ReverseGrid.Add(i_Tile, i_Coordinates);

        mapTile(i_Tile.PositiveZNeighbour, i_Coordinates + Vector2Int.up);
        mapTile(i_Tile.NegativeZNeighbour, i_Coordinates - Vector2Int.up);
        mapTile(i_Tile.PositiveXNeighbour, i_Coordinates + Vector2Int.right);
        mapTile(i_Tile.NegativeXNeighbour, i_Coordinates - Vector2Int.right);
        mapTile(i_Tile.PositiveYNeighbour, i_Coordinates - Vector2Int.right + Vector2Int.up);
        mapTile(i_Tile.NegativeYNeighbour, i_Coordinates + Vector2Int.right - Vector2Int.up);
    }

    /// <summary>
    /// Gets the coordinates of the given tile.
    /// </summary>
    public Vector2Int GetCoordinatesFor(HexTile i_Tile)
    {
        return m_ReverseGrid[i_Tile];
    }

    /// <summary>
    /// Gets the distance between the 2 given tiles.
    /// </summary>
    public int GetDistanceBetween(HexTile i_Tile1, HexTile i_Tile2)
    {
        //To properly calculate the distance, we need to use the z coordinate as well, which we're emitting with our hex system.
        //However, in this system, x+y+z=0, so the z coordinate can be derived from the x and y values.
        Vector3Int coordsTile1 = new Vector3Int(i_Tile1.Coordinates.x, i_Tile1.Coordinates.y, -i_Tile1.Coordinates.x - i_Tile1.Coordinates.y);
        Vector3Int coordsTile2 = new Vector3Int(i_Tile2.Coordinates.x, i_Tile2.Coordinates.y, -i_Tile2.Coordinates.x - i_Tile2.Coordinates.y);

        return Mathf.Max(Mathf.Abs(coordsTile2.x - coordsTile1.x), 
                         Mathf.Abs(coordsTile2.y - coordsTile1.y), 
                         Mathf.Abs(coordsTile2.z - coordsTile1.z));
    }
}