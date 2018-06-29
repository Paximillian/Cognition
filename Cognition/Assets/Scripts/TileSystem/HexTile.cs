using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class HexTile : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    #region Variables
    private NetworkIdentity m_NetId;
    
    [SerializeField]
    bool m_DrivingCog = false;
    public bool DrivingCog { get { return m_DrivingCog; } set { m_DrivingCog = value; } }

    [HideInInspector]
    [SerializeField]
    private Cog m_ResidentCog;
    public Cog ResidentCog
    {
        get { return m_ResidentCog; }
        set
        {
            if (m_ResidentCog = value)
            {
                m_ResidentCogSceneId = value.GetComponent<NetworkIdentity>()?.netId ?? new NetworkInstanceId(0);
            }
        }
    }

    [ReadOnly]
    [SerializeField]
    private HexTile m_NeighborXPos;
    public HexTile PositiveXNeighbour { get { return m_NeighborXPos; } set { m_NeighborXPos = value; } }
    [ReadOnly]
    [SerializeField]
    private HexTile m_NeighborXNeg;
    public HexTile NegativeXNeighbour { get { return m_NeighborXNeg; } set { m_NeighborXNeg = value; } }
    [ReadOnly]
    [SerializeField]
    private HexTile m_NeighborYPos;
    public HexTile PositiveYNeighbour { get { return m_NeighborYPos; } set { m_NeighborYPos = value; } }
    [ReadOnly]
    [SerializeField]
    private HexTile m_NeighborYNeg;
    public HexTile NegativeYNeighbour { get { return m_NeighborYNeg; } set { m_NeighborYNeg = value; } }
    [ReadOnly]
    [SerializeField]
    private HexTile m_NeighborZPos;
    public HexTile PositiveZNeighbour { get { return m_NeighborZPos; } set { m_NeighborZPos = value; } }
    [ReadOnly]
    [SerializeField]
    private HexTile m_NeighborZNeg;
    public HexTile NegativeZNeighbour { get { return m_NeighborZNeg; } set { m_NeighborZNeg = value; } }
    
    [SyncVar(hook = "onAssignedResidentCog")]
    private NetworkInstanceId m_ResidentCogSceneId;
    private void onAssignedResidentCog(NetworkInstanceId i_CogSceneId)
    {
        m_ResidentCogSceneId = i_CogSceneId;
        ResidentCog = ClientScene.FindLocalObject(m_ResidentCogSceneId).GetComponent<Cog>();
    }

    private List<HexTile> m_Neighbors;

    /// <summary>
    /// The coordinates of this tile on the grid.
    /// </summary>
    public Vector2Int Coordinates { get; set; }

    /// <summary>
    /// To properly calculate the distance, we need to use the z coordinate as well, which we're emitting with our hex system.
    /// However, in this system, x+y+z=0, so the z coordinate can be derived from the x and y values.
    /// </summary>
    private Vector3Int FullCoordinates => new Vector3Int(Coordinates.x, Coordinates.y, -Coordinates.x - Coordinates.y);

    /// <summary>
    /// Gets the distance between the 2 given tiles.
    /// </summary>
    public Func<HexTile, int> DistanceTo
    {
        get
        {
            return (tile) =>
            {
                return Mathf.Max(Mathf.Abs(FullCoordinates.x - tile.FullCoordinates.x),
                                 Mathf.Abs(FullCoordinates.y - tile.FullCoordinates.y),
                                 Mathf.Abs(FullCoordinates.z - tile.FullCoordinates.z));
            };
        }
    }

    /// <summary>
    /// The tiles neighbouring this tile.
    /// </summary>
    public List<HexTile> Neighbors 
    {
        get
        {
            if (m_Neighbors == null)
            {
                m_Neighbors = new List<HexTile>();
                if (m_NeighborXPos != null)
                    m_Neighbors.Add(m_NeighborXPos);
                if (m_NeighborXNeg != null)
                    m_Neighbors.Add(m_NeighborXNeg);
                if (m_NeighborYPos != null)
                    m_Neighbors.Add(m_NeighborYPos);
                if (m_NeighborYNeg != null)
                    m_Neighbors.Add(m_NeighborYNeg);
                if (m_NeighborZPos != null)
                    m_Neighbors.Add(m_NeighborZPos);
                if (m_NeighborZNeg != null)
                    m_Neighbors.Add(m_NeighborZNeg);
            }
            return m_Neighbors;
        }
    }

    public IEnumerable<HexTile> GetHexTilesInRadius(int searchRadius)
    {
        HashSet<HexTile> resTiles = new HashSet<HexTile>();
        resTiles.Add(this);

        for (int i = 0; i < searchRadius; i++)
        {
            List<HexTile> tilesToAdd = resTiles.SelectMany(tile => tile.Neighbors).ToList();

            if (resTiles.IsSupersetOf(tilesToAdd)) { break; }
            else { resTiles.AddRange(tilesToAdd); }
        }

        return resTiles;
    }

    /// <summary>
    /// Gets all the cogs that are within the given radius from this tile.
    /// This is different than finding cogs by distance, which only finds cogs that can be reached by an existing path.
    /// </summary>
    public Func<int, IEnumerable<Cog>> PopulatedNeighborsInRadius =>
        ((radius) => GetHexTilesInRadius(radius - 1)
                        .SelectMany((neighbor) => neighbor.PopulatedNeighbors)
                        .Where(cog => !cog.Equals(ResidentCog)));

    /// <summary>
    /// Gets all the cogs that are reachable from this cog in the given distance.
    /// This is different than finding cogs by radius, which can travel over missing tiles.
    /// </summary>
    public Func<int, IEnumerable<Cog>> PopulatedNeighborsInDistance =>
        ((distance) => PopulatedNeighborsInRadius(distance)
                         .Where(cog => this.DistanceTo(cog.HoldingTile) <= distance));

    /// <summary>
    /// The cogs on the tiles neighbouring this tile.
    /// </summary>
    public IEnumerable<Cog> PopulatedNeighbors //Could cache this for performance sake if tile are static through out the game
    {
        get
        {
            return Neighbors.Where((neighbor) => neighbor.ResidentCog != null)
                            .Select((tile) => tile.ResidentCog);
        }
    }
    #endregion Variables

    #region UnityMethods
    private void Awake()
    {
        m_NetId = GetComponent<NetworkIdentity>();
    }
    #endregion UnityMethods

    #region UNETMethods
    [Server]
    public void DestroyCog(bool initialBaseCog = false)
    {
        if (ResidentCog)
        {
            NetworkPlayer owningPlayer = (ResidentCog as PlayableCog)?.OwningPlayer;
            if (owningPlayer)
            {
                owningPlayer.OwnedCogs.Remove(ResidentCog);
            }

            //Triggers breakdown abilities
            foreach (Cog neighbour in ResidentCog.PropagationStrategy.Neighbors)
            {
                neighbour.InvokeDisconnectionAbilities(ResidentCog);
            }
            ResidentCog.InvokeBreakdownAbilities();

            ResidentCog.ResetCog();
            ResidentCog = null;
            if (!initialBaseCog) { Rpc_RemoveResidentCog(); } //For client

            owningPlayer?.PlayerBaseCog.PropagationStrategy.InitializePropagation(owningPlayer, null, true);
        }
    }

    [ClientRpc]
    private void Rpc_RemoveResidentCog()
    {
        if (!isServer)
        {
            ResidentCog?.UpdateSpin(ResidentCog.Spin = 0);
        }

        ResidentCog = null;
    }
    #endregion UNETMethods

    #region PublicMethods
    public HexTile GetRelativeTile(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            return this;
        }
        if (direction.x > 0)
        {
            return m_NeighborXPos.GetRelativeTile(direction - Vector3.right);
        }
        else if (direction.x < 0)
        {
            return m_NeighborXNeg.GetRelativeTile(direction + Vector3.right);
        }
        else if (direction.y > 0)
        {
            return m_NeighborYPos.GetRelativeTile(direction - Vector3.up);
        }
        else if (direction.y < 0)
        {
            return m_NeighborYNeg.GetRelativeTile(direction + Vector3.up);
        }
        else if (direction.z > 0)
        {
            return m_NeighborZPos.GetRelativeTile(direction - Vector3.forward);
        }
        else if (direction.z < 0)
        {
            return m_NeighborZNeg.GetRelativeTile(direction + Vector3.forward);
        }
        throw new UnityException("sanity check failed we went mad, moo");
    }
    #endregion PublicMethods

    #region UIInteractivity
    public void OnPointerDown(PointerEventData eventData)
    {
        if (NetworkPlayer.Server?.GameStarted ?? false)
        {
            eventData.position = Camera.main.WorldToScreenPoint(transform.Find("tile_cog_connection").position);
            RadialMenuController.Instance.OnPointerDown(eventData, this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        RadialMenuController.Instance.OnPointerUp(eventData, this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RadialMenuController.Instance.OnDrag(eventData, this);
    }
    #endregion UIInteractivity
}
