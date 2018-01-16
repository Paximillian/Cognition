using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer LocalPlayer { get; private set; }
    public bool IsReady { get; private set; }

    private NetworkIdentity m_NetId;

    [SyncVar(hook = "assignedPlayerId")]
    private int m_PlayerId;
    public int PlayerId { get { return m_PlayerId; } set { m_PlayerId = value; } }

    [SyncVar(hook = "onResourcesChanged")]
    [SerializeField]
    private int m_Resources = 0;
    public int Resources { get { return m_Resources; } internal set { m_Resources = value; } }

    /// <summary>
    /// The cogs owned by this player.
    /// </summary>
    public HashSet<BaseCog> OwnedCogs { get; private set; } = new HashSet<BaseCog>();

    private Text m_ResourcesUIText;

    [SyncVar(hook = "onPlayerNicknameChanged")]
    private string m_Nickname = "Nope";

    private void assignedPlayerId(int i_PlayerId)
    {
        m_PlayerId = i_PlayerId;
    }

    public BaseCog PlayerBaseCog { get; set; }


    public HashSet<BaseCog> updatedCogs = new HashSet<BaseCog>();



    /// <summary>
    /// How many players have been added to the game already.
    /// </summary>
    private static int s_LoadedPlayers = 0;

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

    /// <summary>
    /// Hook called when the amount of resources of this player changes.
    /// </summary>
    private void onResourcesChanged(int i_Resources)
    {
        m_Resources = i_Resources;

        if (isLocalPlayer)
        {
            if(m_ResourcesUIText == null)
            {
                m_ResourcesUIText = GameObject.FindGameObjectWithTag("ResourcesUIText")?.GetComponent<Text>();
            }

            m_ResourcesUIText.text = m_Resources.ToString();
        }
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
                
                tile.BuildCog(m_NetId, "Cog_Player");
                PlayerBaseCog = tile.ResidentCog;
                tile.ResidentCog.OccupyingPlayers.Add(this);
            }
        }
    }

    IEnumerator waitForRoleUpdate()
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

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        LocalPlayer = this;
        Cmd_SetReady();
        Cmd_SetNickname(PlayerPrefs.GetString("Nickname", "Nope"));
        NamesManager.Instance.LocalName = m_Nickname;
    }

    protected virtual void onStartNonLocalPlayer()
    {
        NamesManager.Instance.OpponentName = m_Nickname;
    }

    private void onPlayerNicknameChanged(string i_Nickvar)
    {
        m_Nickname = i_Nickvar;
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

        tile.BuildCog(i_PlacingPlayer, i_CogPrefabName);
    }

    [Command]
    private void Cmd_SetNickname(string i_Nickname)
    {
        m_Nickname = i_Nickname;
    }
}
