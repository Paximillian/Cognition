
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkPlayer : NetworkBehaviour
{
    #region Variables
    public static NetworkPlayer LocalPlayer { get; private set; }
    public static NetworkPlayer Server { get; private set; }
    
    /// <summary>
    /// Called when the amount of resources this player collected changes.
    /// </summary>
    public event Action<int> ResourcesChanged;

    /// <summary>
    /// Called when this player successfully finishes building a cog.
    /// </summary>
    public event Action<Cog> CogBuilt;

    public bool IsReady { get; private set; }

    private NetworkIdentity m_NetId;

    private CameraController m_CameraController;

    /// <summary>
    /// The camera controlled by this player.
    /// </summary>
    private CameraController CameraController
    {
        get
        {
            if (m_CameraController == null)
            {
                m_CameraController = FindObjectOfType<CameraController>();
            }

            return m_CameraController;
        }
    }

    [SyncVar(hook = "assignedPlayerId")]
    private int m_PlayerId;
    public int PlayerId { get { return m_PlayerId; } private set { m_PlayerId = value; } }
    private void assignedPlayerId(int i_PlayerId)
    {
        m_PlayerId = i_PlayerId;
    }

    [SyncVar(hook = "onResourcesChanged")]
    [SerializeField]
    private int m_Resources = 0;
    public int Resources { get { return m_Resources; } set { m_Resources = value; } }
    /// <summary>
    /// Hook called when the amount of resources of this player changes.
    /// </summary>
    private void onResourcesChanged(int i_Resources)
    {
        m_Resources = i_Resources;

        if (isLocalPlayer)
        {
            ResourceCountLabel.Instance.Label.text = m_Resources.ToString();

            ResourcesChanged?.Invoke(m_Resources);
        }
    }

    /// <summary>
    /// The cogs owned by this player.
    /// </summary>
    public HashSet<Cog> OwnedCogs { get; private set; } = new HashSet<Cog>();

    /// <summary>
    /// Has the game started already?
    /// </summary>
    public bool GameStarted { get { return m_GameStarted; } private set { m_GameStarted = value; } }
    [SyncVar]
    private bool m_GameStarted = false;
    
    /// <summary>
    /// The text displayed on the countdown.
    /// </summary>
    [SyncVar(hook = "onCountdownTextChanged")]
    private string m_CountdownText;

    [SyncVar(hook = "onPlayerNicknameChanged")]
    private string m_Nickname = "Nope";
    private void onPlayerNicknameChanged(string i_Nickvar)
    {
        m_Nickname = i_Nickvar;
    }

    public Cog PlayerBaseCog { get; set; }


    public HashSet<Cog> UpdatedCogs { get; private set; } = new HashSet<Cog>();

    /// <summary>
    /// How many players have been added to the game already.
    /// </summary>
    private static int s_LoadedPlayers = 0;
    #endregion Variables

    #region UnityMethods
    private void Awake()
    {
        m_NetId = GetComponent<NetworkIdentity>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(updateInitialPosition());
    }

    private void OnDestroy()
    {
        s_LoadedPlayers = 0;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isServer)
        {
            s_LoadedPlayers++;
        }

        StartCoroutine(waitForRoleUpdate());
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        LocalPlayer = this;
        Cmd_SetReady();
        Cmd_SetNickname(PlayerPrefs.GetString("Nickname", "Nope"));
        NamesManager.Instance.LocalName = m_Nickname;

        if (isServer)
        {
            Server = this;
        }
    }
    #endregion UnityMethods

    #region PrivateMethods
    private void onCountdownTextChanged(string i_Text)
    {
        m_CountdownText = i_Text;
        CountdownLabel.Instance.Label.text = m_CountdownText;

        if (m_CountdownText.Equals("Go!") || String.IsNullOrWhiteSpace(m_CountdownText))
        {
            CameraController?.Enable();
        }
        else
        {
            CameraController?.Disable();
        }
    }

    /// <summary>
    /// Starts the game 3 seconds after both players connect.
    /// </summary>
    [Server]
    private IEnumerator countDownToGameStart()
    {
        Rpc_SetServerPlayer();
        yield return new WaitForSeconds(1f);

        for (int i = 3; i > 0; --i)
        {
            m_CountdownText = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        m_CountdownText = "Go!";
        GameStarted = true;

        yield return new WaitForSeconds(1f);

        m_CountdownText = string.Empty;
    }

    /// <summary>
    /// Initializes the player in a valid starting position and sets up their initial cog.
    /// </summary>
    private IEnumerator updateInitialPosition()
    {
        NetworkStartPosition[] startPositions = null;
        
        if (isServer)
        {
            while (NetworkObjectPoolManager.Instance == null)
            {
                yield return null;
            }

            //Finds a valid starting position in the scene.
            do
            {
                startPositions = GameObject.FindObjectsOfType<NetworkStartPosition>();
                yield return null;
            } while (startPositions?.Length == 0);

            //Sets the player's position
            startPositions[s_LoadedPlayers].gameObject.GetComponentInChildren<SetInGameNickname>(true).SetInGameName(isLocalPlayer);
            transform.position = startPositions[s_LoadedPlayers++].transform.position;
            m_PlayerId = s_LoadedPlayers;

            //We then build our initial cog on this tile.
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, -Vector3.up, out hit, 2, LayerMask.GetMask("HexTile")))
            {
                HexTile tile = hit.collider.GetComponent<HexTile>();
                tile.DrivingCog = true;
                tile.DestroyCog(true);

                BuildCog(tile, m_NetId, "Cog_Player");
            }
            else
            {
                Debug.LogError("Couldn't find tile to put the player cog on");
            }

            if (s_LoadedPlayers == NetworkManager.singleton.matchSize)
            {
                Server.StartCoroutine(Server.countDownToGameStart());
            }
        }
    }

    private IEnumerator waitForRoleUpdate()
    {
        while (s_LoadedPlayers < 2)
        {
            yield return null;
        }

        if (!Equals(LocalPlayer))
        {
            onStartNonLocalPlayer();
        }
    }

    protected virtual void onStartNonLocalPlayer()
    {
        NamesManager.Instance.OpponentName = m_Nickname;
    }
    #endregion PrivateMethods

    #region UNETMethods
    /// <summary>
    /// Sets this as the server player.
    /// </summary>
    [ClientRpc]
    private void Rpc_SetServerPlayer()
    {
        Server = this;
    }

    /// <summary>
    /// Requests from the server to build a cog on the board.
    /// </summary>
    [Client]
    public void BuildCog(HexTile i_Tile, Cog i_CogPrefab)
    {
        if (Resources >= i_CogPrefab.Cost)
        {
            BuildCogRequest(i_Tile.GetComponent<NetworkIdentity>(), i_CogPrefab.gameObject.name);
        }
    }

    /// <summary>
    /// Checks if this player can build the given cog on the given tile.
    /// </summary>
    [Client]
    public bool CanBuildCog(HexTile i_Tile, Cog i_CogPrefab)
    {
        //TODO: Add build check logic here
        if (i_Tile?.ResidentCog != null) {//If there's already a cog here we can't build
            return false;
        }

        if (i_CogPrefab.BuildRange == int.MaxValue || i_Tile == null) {//unlimited range cog or tile irrelevant
            return true;
        }

        return i_Tile.PopulatedNeighborsInRadius(i_CogPrefab.BuildRange) //Allow building if one of these:
            .Where(cog => ((((cog as PlayableCog)?.OwningPlayer.Equals(this) ?? false) //1) Is a cog this player owns
             && ((cog as PlayableCog)?.BuildRange != int.MaxValue)) //and it's not a global range cog
            || ((cog as NeutralCog)?.OccupyingPlayers.Contains(this) ?? false)) //2) Is a neutral cog this player controls
            //&& (cog.BuildRange != Mathf.Infinity || cog.Spin != 0f) //For global summon cogs TODO: examine infinity properly
            //3) Is a neutral cog that has an adjacent moving cog owned by this player (for client) !CURRENTLY DISABLED DON@T DELETE BEFORE TESTING
            //|| ((cog as NeutralCog)?.HoldingTile.PopulatedNeighbors
            //.Where(neighbor => ((neighbor as PlayerCog)?.OwningPlayer.Equals(this) ?? false) && (neighbor.Spin != 0f)).Count() > 0)
            ).Count() > 0;
    }

    /// <summary>
    /// Actually builds the cog on the server side.
    /// </summary>
    [Server]
    public PlayableCog BuildCog(HexTile i_Tile, NetworkIdentity i_PlacingPlayer, string i_CogPrefabName)
    {
        PlayableCog cog = null;
        NetworkPlayer placingPlayer = ClientScene.FindLocalObject(i_PlacingPlayer.netId).GetComponent<NetworkPlayer>();

        if (i_Tile.ResidentCog == null)
        {
            cog = NetworkObjectPoolManager.PullObject(i_CogPrefabName).GetComponent<PlayableCog>();

            if (placingPlayer.Resources >= cog.Cost)
            {
                placingPlayer.Resources -= cog.Cost;

                cog.transform.position = i_Tile.transform.position;
                cog.HoldingTile = i_Tile;
                i_Tile.ResidentCog = cog;
                cog.OwningPlayer = placingPlayer;
                cog.OwningPlayerId = placingPlayer.PlayerId;

                cog.transform.position += (i_Tile.transform.Find("tile_cog_connection").position - cog.transform.Find("tile_cog_connection").position);

                if (placingPlayer.PlayerBaseCog == null)
                {
                    placingPlayer.PlayerBaseCog = cog;
                    cog.OccupyingPlayers.Add(placingPlayer);
                }

                placingPlayer.OwnedCogs.Add(cog);

                cog.PropagationStrategy.InitializePropagation(placingPlayer, null);//Was null before

                //Invokes abilities based on building of this cog.
                foreach (Cog neighbour in cog.PropagationStrategy.Neighbors)
                {
                    neighbour.InvokeConnectionAbilities(cog);
                }
                cog.InvokeBootupAbilities();

                Rpc_CogBuilt(cog.netId);
            }
            else
            {
                cog.gameObject.SetActive(false);
            }

            if (i_Tile.DrivingCog)
            {
                cog.RequestUpdateSpin(1f);//TODO make this place a value in accordance to spin wanted
            }
        }

        return cog;
    }

    /// <summary>
    /// Called on the client when a cog finishes building.
    /// </summary>
    [ClientRpc]
    private void Rpc_CogBuilt(NetworkInstanceId i_BuiltCogId)
    {
        if (isLocalPlayer)
        {
            Cog cog = ClientScene.FindLocalObject(i_BuiltCogId).GetComponent<Cog>();

            CogBuilt?.Invoke(cog);
        }
    }

    [Command]
    private void Cmd_SetReady()
    {
        IsReady = true;
    }

    /// <summary>
    /// Routes a request to build a cog on the tile represented by the given net id.
    /// </summary>
    public void BuildCogRequest(NetworkIdentity i_TileToBuildOn, string i_CogName)
    {
        Cmd_BuildCog(m_NetId, i_TileToBuildOn, i_CogName);
    }

    /// <summary>
    /// Builds a cog on the board for all players to see.
    /// </summary>
    [Command]
    private void Cmd_BuildCog(NetworkIdentity i_PlacingPlayer, NetworkIdentity i_TileToBuildOn, string i_CogPrefabName)
    {
        HexTile tile = ClientScene.FindLocalObject(i_TileToBuildOn.netId).GetComponent<HexTile>();

        BuildCog(tile, i_PlacingPlayer, i_CogPrefabName);
    }

    [Command]
    private void Cmd_SetNickname(string i_Nickname)
    {
        m_Nickname = i_Nickname;
    }
    #endregion UNETMethods
}
