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
                placingPlayer.PlayerBaseCog.PropagationStrategy.Propogate(placingPlayer, null); ;
            }
            else
            {
                cog.gameObject.SetActive(false);
            }        }
        if (m_DrivingCog) {
            Rpc_UpdateSpinInitial(m_spin = 1f);//TODO make this place a value in accordance to spin wanted
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

    /// <summary>
    /// This update the logic for a single tile, and propagates if neccessery.
    /// </summary>
    /// <param name="propogate"></param>
    /// <param name="delayedPropogation"></param>
    /// <param name="forceStop"></param>
    /// <returns></returns>
    public bool UpdateTile(bool propogate = false, bool delayedPropogation = true, bool forceStop = false, NetworkPlayer i_NetPlayer = null) //Return value indicates if the tiles status was changed or not
    {
        float prevSpin = m_spin;
        bool statusChanged = false;
        if (forceStop)  //ForceStop means we are in the first pass of a cog being destroyed, we should stop in any case.
        {
            Rpc_UpdateSpin(m_spin = 0f);
            Rpc_StopConflicted();

            if (propogate)
            {
                StartCoroutine(BFSUpdateDriven(ResidentCog?.OwningPlayer, false));
            }
            return true;
        }

        if (ResidentCog == null)    //In this case the cog here was destroyed, update neighbors only
        {
            StartCoroutine(BFSUpdateDriven(i_NetPlayer ? i_NetPlayer : ResidentCog?.OwningPlayer, false));
            return true;
        }

        List<HexTile>   spiners     = new List<HexTile>();
        bool            conflicting = false;
        
        foreach (HexTile nbr in Neighbors)
        {
            if (nbr.ResidentCog != null)
            {
                if (Mathf.Abs(nbr.m_spin) > 0.5f)
                {
                    if (spiners.Count == 0) {
                        Rpc_UpdateSpin(m_spin = -nbr.m_spin);
                    }
                    spiners.Add(nbr);
                    conflicting = conflicting || isAdjacentToAny(nbr, spiners);//(m_spin == nbr.m_spin);  
                }
            }
        }

        if (ResidentCog.OwningPlayer != null)
        {
            List<NetworkPlayer> owners = ownersPresent(spiners);
            conflicting = conflicting || ((owners.Contains(ResidentCog.OwningPlayer) && owners.Count > 1) || (!owners.Contains(ResidentCog.OwningPlayer) && owners.Count > 0)); //is there an enemy spiner here too?
        }
        statusChanged = prevSpin != m_spin;

        if (spiners.Count == 0)
        {
            Rpc_UpdateSpin(m_spin = 0f); //If spin is 0 we can stop conflict inside updateSpin
            Rpc_StopConflicted();
            
            return false;
        }
        else
        {
            if (conflicting) //make conflicted
            {
                //foreach (HexTile nbr in spiners) //Includes tiles that dont really need to be conflicted, TODO: fix dis pls.
                //{
                    //if (nbr.Spin == m_spin)
                    //{
                        //nbr.Rpc_MakeConflicted();
                    //}
                //}
                Rpc_MakeConflicted();
            }
            else
            {
                Rpc_StopConflicted();
            }
            if(propogate)
            {
                StartCoroutine(BFSUpdateDriven(ResidentCog?.OwningPlayer, delayedPropogation));
            }
        }
        return statusChanged;
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

            Rpc_StopConflicted();
            Rpc_UpdateSpin(m_spin = 0f);
            UpdateTile(true, false, false, owningPlayer);
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

    /*float IsDrivenTemp() //Temp method to use while proper system isn't ready
    {
        float calculatedSpin = 0;

        if (DrivingCog)
        {
            return m_spin;
        }

        ResidentCog?.OccupyingPlayers.Clear();
        float spinMultiplier = 1f;
        HexTile current;
        Queue frontier = new Queue();
        frontier.Enqueue(this);
        List<HexTile> visited = new List<HexTile>();
        visited.Add(this);
        while (frontier.Count > 0)
        {
            current = (HexTile)frontier.Dequeue();
            spinMultiplier *= -1f;
            foreach (HexTile adj in current.Neighbors)
            {
                if (!visited.Contains(adj) && adj.ResidentCog!= null) {
                    //Debug.Log("Checking tile " + adj.gameObject.name);
                    if (adj.DrivingCog)
                    {
                        //Debug.Log("Found a driver " + adj.gameObject.name);
                        if (ResidentCog.OwningPlayer == null) {
                            ResidentCog?.OccupyingPlayers.Add(adj.ResidentCog.OwningPlayer);
                        }
                        calculatedSpin = spinMultiplier * adj.Spin;
                    }
                    frontier.Enqueue(adj);
                    visited.Add(adj);
                }
            }

        }
        return calculatedSpin;
    }*/

   //public void MakeConflictedSingle() {
      //  ResidentCog.transform.Rotate(Vector3.right * 0.5f);
    //}

    [ClientRpc]
    private void Rpc_MakeConflicted()
    {
        if (!DrivingCog)
        {
            MakeConflicted();
        }
    }

    public void MakeConflicted()
    {
        //ResidentCog.transform.Translate(Vector3.up * 0.5f);
        //Debug.Log("making conflicted");
        m_conflicted = true;
        if (ResidentCog) {
            ResidentCog.transform.localScale = Vector3.one + Vector3.right * 0.2f;
        }
        StartCoroutine(DealConflictDamage());
    }

    [ClientRpc]
    private void Rpc_StopConflicted()
    {
        StopConflicted();
    }

    public void StopConflicted()
    {
        //ResidentCog.transform.Translate(Vector3.up * 0.5f);
        m_conflicted = false;
        if (ResidentCog)
        {
            ResidentCog.transform.localScale = Vector3.one;
        }
    }

    IEnumerator DealConflictDamage(float damage = 1f)
    {
        while (m_conflicted && ResidentCog)
        {
            ResidentCog?.DealDamage(Time.deltaTime * damage);
            yield return new WaitForEndOfFrame();
        }
        Rpc_StopConflicted();
    }

    void ActivateBFSUpdateDriven(bool delayedReaction = true, HexTile neighbor = null)
    {
        StartCoroutine(BFSUpdateDriven(neighbor?.ResidentCog?.OwningPlayer, delayedReaction, neighbor));
    }

    IEnumerator BFSUpdateDriven(NetworkPlayer i_OriginPlayer, bool delayedReaction = true, HexTile i_HexTile = null) //TODO fix this while not bottlenecked
    {
        while (s_BFSsRunning > 0)
        {
            yield return null;
        }
        s_BFSsRunning++;

        HexTile current;
        Queue frontier = new Queue();
        List<HexTile> visited = new List<HexTile>();

        frontier.Enqueue((i_HexTile == null) ? this : i_HexTile);
        visited.Add((i_HexTile == null) ? this : i_HexTile);

        while (frontier.Count > 0) //BFS loop
        {
            current = (HexTile)frontier.Dequeue();
            foreach (HexTile neighbor in current.Neighbors)
            {
                if (!visited.Contains(neighbor) && neighbor.ResidentCog != null)
                {
                    //instead of clearing activate new BFS cause previous one hasnt nessacceraly finished
                    if (!delayedReaction && neighbor.m_DrivingCog)
                    {
                        //We hit a driver cog everything back on
                        ActivateBFSUpdateDriven(false, neighbor);
                    }
                    else
                    {
                        if (neighbor.UpdateTile(false, delayedReaction, current.Spin == 0f)) //If this tiles status hasn't changed theres no need to update it's neighbors
                        {
                            if (neighbor.ResidentCog.OwningPlayer?.Equals(i_OriginPlayer) ?? false || neighbor.ResidentCog.OwningPlayer == null)
                            {
                                if (neighbor.ResidentCog.OwningPlayer == null && i_OriginPlayer != null)
                                {
                                    neighbor.ResidentCog.OccupyingPlayers.Add(i_OriginPlayer);
                                }
                                frontier.Enqueue(neighbor);
                            }

                        }
                        visited.Add(neighbor);
                    }
                }
            }

            if (delayedReaction)
            {
                yield return new WaitForSeconds(0.1f); //Parameterize delay amount
            }
        }
        s_BFSsRunning--;
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
