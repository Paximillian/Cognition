using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class HexTile : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private BaseCog m_ResidentCog;
    public BaseCog ResidentCog { get { return m_ResidentCog; } private set { m_ResidentCog = value; } }

    private NetworkIdentity m_NetId;

    [SerializeField] bool m_DrivingCog = false;
    public bool DrivingCog { get { return m_DrivingCog; } set { m_DrivingCog = value; } }

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

    List<HexTile> m_neighbors;

    [SerializeField]
    
    float m_spin = 0f;

    public float Spin{get {return m_spin;}}

    bool m_conflicted = false;
    [SerializeField]
    bool simulateSearch = false;

    public int s_BFSsRunning = 0;

    [SyncVar(hook = "onAssignedResidentCog")]
    private NetworkInstanceId m_ResidentCogSceneId;
    private void onAssignedResidentCog(NetworkInstanceId i_CogSceneId)
    {
        m_ResidentCogSceneId = i_CogSceneId;
        ResidentCog = ClientScene.FindLocalObject(m_ResidentCogSceneId).GetComponent<BaseCog>();
    }

    List<HexTile> Neighbors //Could cache this for performance sake if tile are static through out the game
    {
        get
        {
            if (m_neighbors == null)
            {
                m_neighbors = new List<HexTile>();
                if (m_NeighborXPos != null)
                    m_neighbors.Add(m_NeighborXPos);
                if (m_NeighborXNeg != null)
                    m_neighbors.Add(m_NeighborXNeg);
                if (m_NeighborYPos != null)
                    m_neighbors.Add(m_NeighborYPos);
                if (m_NeighborYNeg != null)
                    m_neighbors.Add(m_NeighborYNeg);
                if (m_NeighborZPos != null)
                    m_neighbors.Add(m_NeighborZPos);
                if (m_NeighborZNeg != null)
                    m_neighbors.Add(m_NeighborZNeg);
            }
            return m_neighbors;
        }
    }

    public IEnumerable<BaseCog> PopulatedNeighbors //Could cache this for performance sake if tile are static through out the game
    {
        get
        {
            return Neighbors.Where((neighbor) => neighbor.ResidentCog != null)
                            .Select((tile) => tile.ResidentCog);
        }
    }




    private void Awake()
    {
        m_NetId = GetComponent<NetworkIdentity>();
    }


    /// <summary>
    /// Requests from the server to build a cog on the board.
    /// </summary>
    [Client]
    public void BuildCog(BaseCog i_CogPrefab)
    {
        if (NetworkPlayer.LocalPlayer.Resources > i_CogPrefab.Cost)
        {
            NetworkPlayer.LocalPlayer.BuildCogRequest(m_NetId, i_CogPrefab.gameObject.name);
        }
    }

    /// <summary>
    /// Actually builds the cog on the server side.
    /// </summary>
    [Server]
    public BaseCog BuildCog(NetworkIdentity i_PlacingPlayer, string i_CogPrefabName)
    {
        BaseCog cog = null;
        NetworkPlayer placingPlayer = ClientScene.FindLocalObject(i_PlacingPlayer.netId).GetComponent<NetworkPlayer>();

        if (ResidentCog == null)
        {
            cog = NetworkObjectPoolManager.PullObject(i_CogPrefabName).GetComponent<BaseCog>();

            if (placingPlayer.Resources > cog.Cost)
            {
                placingPlayer.Resources -= cog.Cost;

                cog.transform.position = transform.position;
                cog.HolderTile = this;
                ResidentCog = cog;
                m_ResidentCogSceneId = cog.GetComponent<NetworkIdentity>().netId;
                cog.OwningPlayer = placingPlayer;
                cog.OwningPlayerId = placingPlayer.PlayerId;

                cog.transform.position += (transform.Find("tile_cog_connection").position - cog.transform.Find("tile_cog_connection").position);
                cog.resetCog();

                placingPlayer.OwnedCogs.Add(cog);
                //placingPlayer.PlayerBaseCog.PropagationStrategy.Propogate(placingPlayer, null);


                cog.PropagationStrategy.Propogate(placingPlayer, null);
            }
            else
            {
                cog.gameObject.SetActive(false);
            }
        }
        if (m_DrivingCog) {
            cog.Rpc_UpdateSpinInitial(cog.Spin = 1f);//TODO make this place a value in accordance to spin wanted
        }
        //Rpc_UpdateSpin(IsDriven());

        return cog;
    }

    [ClientRpc]
    private void Rpc_UpdateSpin(float i_SpinAmount)
    {
        if (!DrivingCog)
        {
            UpdateSpin(i_SpinAmount);
        }
    }

    [ClientRpc]
    private void Rpc_UpdateSpinInitial(float i_SpinAmount)
    {
        UpdateSpin(i_SpinAmount);
    }

    public void UpdateSpin(float spin) {
        //if (gameObject.name.Contains("4")) { Debug.Log(" 4 was spun " + spin); }
        StartCoroutine(updateSpin(spin));
    }

    private IEnumerator updateSpin(float spin)
    {
        m_spin = spin;

        Animator animator = null;
        do
        {
            yield return null;
            animator = ResidentCog?.Animator;//Will this not run forever on an empty tile?
        } while (animator == null);

        animator.SetFloat("Spin", m_spin);
    }

    List<NetworkPlayer> ownersPresent(List<HexTile> tiles) {
        List<NetworkPlayer> res = new List<NetworkPlayer>();
        foreach (HexTile tile in tiles) {
            if (tile.ResidentCog?.OwningPlayer != null && !res.Contains(tile.ResidentCog.OwningPlayer)) {
                res.Add(tile.ResidentCog.OwningPlayer);
            }
        }
        return res;
    }

    bool areAdjacent(HexTile a, HexTile b) {
        return a.Neighbors.Contains(b);
    }

    bool isAdjacentToAny(HexTile tile, List<HexTile> candidateList)
    {
        bool res = false;

        foreach (HexTile other in candidateList) {
            res = res || areAdjacent(tile, other);
        }
        return res;
    }

    [Server]
    public void DestroyCog()
    {
        if (ResidentCog)
        {
            NetworkPlayer owningPlayer = ResidentCog.OwningPlayer;
            if (owningPlayer)
            {
                owningPlayer.OwnedCogs.Remove(ResidentCog);
            }
            ResidentCog.InvokeDeathrattle();
            ResidentCog = null;
            
            owningPlayer?.PlayerBaseCog.PropagationStrategy.Propogate(owningPlayer, null, true);

        }
    }

#if UNITY_EDITOR
    public BaseCog Editor_BuildCog(BaseCog i_CogPrefab)
    {
        BaseCog cog = null;
        if (ResidentCog == null)
        {
            cog = (UnityEditor.PrefabUtility.InstantiatePrefab(i_CogPrefab.gameObject) as GameObject).GetComponent<BaseCog>();
            cog.transform.position = transform.position;
            cog.HolderTile = this;

            UnityEditor.Undo.RecordObject(this, "Updated resident cog");
            ResidentCog = cog;

            cog.transform.position += Vector3.up * (transform.Find("tile_cog_connection").position.y - cog.transform.Find("tile_cog_connection").position.y);
        }
        return cog;
    }

    public void Editor_DestroyCog()
    {
        DestroyImmediate(ResidentCog.gameObject);
        ResidentCog = null;
    }
#endif

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
}
