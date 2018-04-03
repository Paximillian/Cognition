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

    [HideInInspector]
    [SerializeField]
    private HexTile m_NeighborXPos;
    public HexTile PositiveXNeighbour { get { return m_NeighborXPos; } set { m_NeighborXPos = value; } }
    [HideInInspector]
    [SerializeField]
    private HexTile m_NeighborXNeg;
    public HexTile NegativeXNeighbour { get { return m_NeighborXNeg; } set { m_NeighborXNeg = value; } }
    [HideInInspector]
    [SerializeField]
    private HexTile m_NeighborYPos;
    public HexTile PositiveYNeighbour { get { return m_NeighborYPos; } set { m_NeighborYPos = value; } }
    [HideInInspector]
    [SerializeField]
    private HexTile m_NeighborYNeg;
    public HexTile NegativeYNeighbour { get { return m_NeighborYNeg; } set { m_NeighborYNeg = value; } }
    [HideInInspector]
    [SerializeField]
    private HexTile m_NeighborZPos;
    public HexTile PositiveZNeighbour { get { return m_NeighborZPos; } set { m_NeighborZPos = value; } }
    [HideInInspector]
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

    public List<HexTile> GetHexTilesInRadius(int searchRadius)
    {
        HashSet<HexTile> resTiles = new HashSet<HexTile>();
        resTiles.Add(this);

        for (int i = 0; i < searchRadius; i++)
        {
            List<HexTile> tilesToAdd = resTiles.SelectMany(tile => tile.Neighbors).ToList();

            if (resTiles.IsSupersetOf(tilesToAdd)) { break; }
            else { resTiles.AddRange(tilesToAdd); }
        }

        return resTiles.ToList();
    }

    public Func<int, List<Cog>> PopulatedNeighborsInRadius =>
        ((radius) => GetHexTilesInRadius(radius - 1)
        .SelectMany((neighbor) => neighbor.PopulatedNeighbors).ToList());

    /// <summary>
    /// The cogs on the tiles neighbouring this tile.
    /// </summary>
    public List<Cog> PopulatedNeighbors //Could cache this for performance sake if tile are static through out the game
    {
        get
        {
            return Neighbors.Where((neighbor) => neighbor.ResidentCog != null)
                            .Select((tile) => tile.ResidentCog)
                            .ToList();
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
    public void DestroyCog()
    {
        if (ResidentCog)
        {
            NetworkPlayer owningPlayer = (ResidentCog as PlayableCog)?.OwningPlayer;
            if (owningPlayer)
            {
                owningPlayer.OwnedCogs.Remove(ResidentCog);
            }
            ResidentCog.InvokeDeathrattle();
            ResidentCog = null;

            owningPlayer?.PlayerBaseCog.PropagationStrategy.InitializePropagation(owningPlayer, null, true);
        }
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
        eventData.position = Camera.main.WorldToScreenPoint(transform.Find("tile_cog_connection").position);
        RadialMenuController.Instance.OnPointerDown(eventData, this);
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
