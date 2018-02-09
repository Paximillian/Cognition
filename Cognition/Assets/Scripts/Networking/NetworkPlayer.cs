using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkPlayer : NetworkBehaviour
{
    #region Variables
    public static NetworkPlayer LocalPlayer { get; private set; }

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
            if (m_ResourcesUIText == null)
            {
                m_ResourcesUIText = GameObject.FindGameObjectWithTag("ResourcesUIText")?.GetComponent<Text>();
            }

            m_ResourcesUIText.text = m_Resources.ToString();

            ResourcesChanged?.Invoke(m_Resources);
        }
    }

    /// <summary>
    /// The cogs owned by this player.
    /// </summary>
    public HashSet<Cog> OwnedCogs { get; private set; } = new HashSet<Cog>();

    private Text m_ResourcesUIText;

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

    public override void OnStartClient()
    {
        base.OnStartClient();

        StartCoroutine(waitForRoleUpdate());
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        LocalPlayer = this;
        Cmd_SetReady();
        Cmd_SetNickname(PlayerPrefs.GetString("Nickname", "Nope"));
        NamesManager.Instance.LocalName = m_Nickname;
    }
    #endregion UnityMethods

    #region PrivateMethods
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
            startPositions[s_LoadedPlayers].gameObject.GetComponentInChildren<SetInGameNickname>().SetInGameName(isLocalPlayer);
            transform.position = startPositions[s_LoadedPlayers++].transform.position;
            m_PlayerId = s_LoadedPlayers;

            //We then build our initial cog on this tile.
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 2, LayerMask.GetMask("HexTile")))
            {
                HexTile tile = hit.collider.GetComponent<HexTile>();
                tile.DrivingCog = true;
                tile.DestroyCog();
                
                BuildCog(tile, m_NetId, "Cog_Player");
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
    /// Requests from the server to build a cog on the board.
    /// </summary>
    [Client]
    public void BuildCog(HexTile i_Tile, Cog i_CogPrefab)
    {
        if (Resources > i_CogPrefab.Cost)
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
        return true;
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
                cog.ResetCog();

                if (placingPlayer.PlayerBaseCog == null)
                {
                    placingPlayer.PlayerBaseCog = cog;
                    cog.OccupyingPlayers.Add(placingPlayer);
                }

                placingPlayer.OwnedCogs.Add(cog);

                cog.PropagationStrategy.InitializePropagation(placingPlayer, null);
                cog.InvokeBattleCry();

                Rpc_CogBuilt(cog.netId);
            }
            else
            {
                cog.gameObject.SetActive(false);
            }
        }
        if (i_Tile.DrivingCog)
        {
            cog.Rpc_UpdateSpin(cog.Spin = 1f);//TODO make this place a value in accordance to spin wanted
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
