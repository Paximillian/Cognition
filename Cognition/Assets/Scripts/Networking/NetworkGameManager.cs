using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

public class NetworkGameManager : NetworkManager
{
    #region Variables
    /// <summary>
    /// The scene to load when a match is found.
    /// </summary>
    [Tooltip("The scene to load when a match is found")]
    [SerializeField]
    private SceneField m_GameScene;

    /// <summary>
    /// If this is checked, we'll run the game in debug mode in a way that we won't clash with other developers' test runs.
    /// </summary>
    public bool DebugMode { get { return m_DebugMode; } set { m_DebugMode = value; } }

    [Tooltip("If this is checked, we'll run the game in debug mode in a way that we won't clash with other developers' test runs.")]
    [SerializeField]
    private bool m_DebugMode = false;


    /// <summary>
    /// Events to fire off when we start looking for a match, use this to update your UI to indicate that match is actively being looked for.
    /// </summary>
    [Tooltip("Events to fire off when we start looking for a match, use this to update your UI to indicate that match is actively being looked for.")]
    [SerializeField]
    private UnityEvent m_OnMatchSearchStarted;

    /// <summary>
    /// Events to fire off when looking for a match fails for any reason.
    /// </summary>
    [SerializeField]
    [Tooltip("Events to fire off when looking for a match fails for any reason.")]
    private UnityEvent m_OnMatchSearchFailed;

    /// <summary>
    /// Events to fire off when a match had been successfully found and joined.
    /// </summary>
    [Tooltip("Events to fire off when a match had been successfully found and joined.")]
    [SerializeField]
    private UnityEvent m_OnMatchFound;

    /// <summary>
    /// A flag to indicate to our system that looking for a match should be cancelled.
    /// </summary>
    private bool m_CancelledMatchmaking = false;

    /// <summary>
    /// A flag to indicate that a player should be spawned for this connection.
    /// </summary>
    private bool m_SpawnPlayers = false;

    /// <summary>
    /// A flag to indicate that we should start over looking for a match we can join.
    /// </summary>
    private bool m_ShouldLookForMatchToJoin = true;

    /// <summary>
    /// A flag to indicate that we're already hosting a match, so that we don't host multiple matches while looking for matches ourselves.
    /// </summary>
    private bool m_IsHostingMatch;

    /// <summary>
    /// We keep this once we start hosting a match so that we can make sure we're not trying to join our own match.
    /// </summary>
    private ulong m_HostedMatchId;

    /// <summary>
    /// A list of active host ids that have joined this match.
    /// </summary>
    private List<int> m_ActiveConnections = new List<int>();

    public string LocalIP
    {
        get
        {
            string ip;

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                ip = endPoint.Address.ToString().Trim();
            }

            return ip;
        }
    }
    #endregion Variables

    #region UnityMethods
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        StartMatchMaker();
    }

    private void OnApplicationQuit()
    {
        if (matchInfo != null)
        {
            StartCoroutine(closeHost());

            NetworkServer.Shutdown();
            StopMatchMaker();
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        base.OnClientConnect(conn);

        Debug.Log("Connected: " + conn.connectionId);
        m_OnMatchFound?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        m_SpawnPlayers = false;
        base.OnServerConnect(conn);

        Debug.Log("Connected to server: " + conn.connectionId);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        //base.OnServerAddPlayer(conn, playerControllerId);

        if (!m_ActiveConnections.Contains(conn.hostId))
        {
            m_ActiveConnections.Add(conn.hostId);
            StartCoroutine(spawnPlayer(conn, playerControllerId));
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        m_CancelledMatchmaking = true;

        base.OnServerSceneChanged(sceneName);

        m_SpawnPlayers = true;
    }
    #endregion UnityMethods

    #region EntryPoints
    /// <summary>
    /// Initializes looking for a match.
    /// </summary>
    public void LookForGame()
    {
        if (matchMaker == null)
        {
            StartMatchMaker();
        }

        m_ActiveConnections.Clear();
        m_HostedMatchId = (ulong)NetworkID.Invalid;
        m_CancelledMatchmaking = false;
        m_ShouldLookForMatchToJoin = true;
        m_OnMatchSearchStarted?.Invoke();

        StartCoroutine(lookForGame());
    }

    /// <summary>
    /// Cancels ongoing matchmaking.
    /// </summary>
    public void CancelLookingForGame()
    {
        StopAllCoroutines();
        StartCoroutine(closeHost());
    }
    #endregion EntryPoints

    #region PrivateMethods
    /// <summary>
    /// We'll actively look for a match alongside creating one and waiting for someone to join ours.
    /// </summary>
    private IEnumerator lookForGame()
    {
        while (!m_CancelledMatchmaking)
        {
            if (m_ShouldLookForMatchToJoin)
            {
                Debug.Log("Looking for more matches...");
                m_ShouldLookForMatchToJoin = false;
                matchMaker?.ListMatches(0, 20, m_DebugMode ? LocalIP : "", false, 0, 0, Matchmaker_OnMatchListReady);
            }

            yield return null;
        }
    }

    /// <summary>
    /// Stops looking for a match.
    /// </summary>
    private IEnumerator closeHost()
    {
        m_IsHostingMatch = false;
        m_CancelledMatchmaking = true;

        yield return new WaitForEndOfFrame();
        yield return null; yield return null;

        yield return matchMaker.SetMatchAttributes(matchInfo.networkId, false, 0, Matchmaker_OnAttributeSet);
        yield return matchMaker.DropConnection(matchInfo.networkId, NodeID.Invalid, 0, Matchmaker_OnConnectionDropped);

        StopHost();
    }

    /// <summary>
    /// Once we get a response for our request to fetch active matches on the matchmaking service, we need to find one we can actually join.
    /// </summary>
    private void Matchmaker_OnMatchListReady(bool i_Success, string i_ExtendedInfo, List<MatchInfoSnapshot> i_ResponseData)
    {
        if (i_Success)
        {
            StartCoroutine(findSuitableMatch(i_ResponseData));
        }
        else
        {
            Debug.LogError("Retrieveing match list failed: " + i_ExtendedInfo);
            m_OnMatchSearchFailed?.Invoke();
            m_ShouldLookForMatchToJoin = true;
        }
    }

    /// <summary>
    /// Looks for a match we can join out of the match list we found earlier.
    /// </summary>
    private IEnumerator findSuitableMatch(List<MatchInfoSnapshot> i_ResponseData)
    {
        MatchInfo foundMatch = null;
        Debug.Log("Matches found: " + i_ResponseData.Count);
        //Looks through all found matches.
        for (int i = 0; i < i_ResponseData.Count && foundMatch == null && !m_CancelledMatchmaking; ++i)
        {
            //Tries to join a match that has room for us.
            if (i_ResponseData[i].currentSize < i_ResponseData[i].maxSize)
            {
                //Make sure we're not trying to join our own match.
                ulong matchId = (ulong)i_ResponseData[i].networkId;
                if (!i_ResponseData[i].networkId.Equals(NetworkID.Invalid) && !matchId.Equals(m_HostedMatchId))
                {
                    //Try to join the potential match, and waits for response.
                    yield return matchMaker.JoinMatch(i_ResponseData[i].networkId, "", "", "", 0, 0, OnMatchJoined);
                }
            }
        }

        //If a match was found, we'll join it, otherwise, we'll create our own.
        if (!m_CancelledMatchmaking)
        {
            if (foundMatch != null)
            {
                closeHost();
                StartClient(foundMatch);
            }
            else
            {
                m_ShouldLookForMatchToJoin = true;

                if (!m_IsHostingMatch)
                {
                    createMatch();
                }
            }
        }
    }

    /// <summary>
    /// Called when a match had been joined.
    /// </summary>
    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            m_CancelledMatchmaking = true;
        }

        base.OnMatchJoined(success, extendedInfo, matchInfo);
    }

    /// <summary>
    /// Creates a new match in the matchmaking service.
    /// </summary>
    private void createMatch()
    {
        m_IsHostingMatch = true;
        matchMaker.CreateMatch(m_DebugMode ? LocalIP : "", 2, true, "", "", "", 0, 0, Matchmaker_OnMatchCreated);
    }

    /// <summary>
    /// Setting an attribute for the match succeeded.
    /// </summary>
    private void Matchmaker_OnAttributeSet(bool success, string extendedInfo)
    {
        Debug.Log("Setting attribute: " + success + ": " + extendedInfo);
    }

    /// <summary>
    /// Once we created a match of our own, we need to host it and listen for connections.
    /// </summary>
    private void Matchmaker_OnMatchCreated(bool success, string extendedInfo, MatchInfo responseData)
    {
        if (success)
        {
            MatchInfo hostInfo = responseData;
            NetworkServer.Listen(hostInfo, networkPort);
            m_HostedMatchId = (ulong)hostInfo.networkId;

            StartHost(hostInfo);
        }
        else
        {
            Debug.LogError("Match creation failed: " + extendedInfo);
            m_OnMatchSearchFailed?.Invoke();
        }
    }

    /// <summary>
    /// Stops matchmaking if a matchmaker server error occurs.
    /// </summary>
    private void Matchmaker_OnConnectionDropped(bool success, string extendedInfo)
    {
        Debug.Log("Dropped connection result: " + success + " " + extendedInfo);

        m_OnMatchSearchFailed?.Invoke();
    }

    /// <summary>
    /// Spawns a player for this connection.
    /// </summary>
    private IEnumerator spawnPlayer(NetworkConnection conn, short playerControllerId)
    {
        while (NetworkServer.connections.Count != matchSize)
        {
            yield return null;
        }

        if (conn.playerControllers.Count == 0)
        {
            Transform spawnPos = GetStartPosition();

            GameObject player;
            if (spawnPos != null)
            {
                player = GameObject.Instantiate(playerPrefab, spawnPos.position, spawnPos.rotation);
            }
            else
            {
                player = GameObject.Instantiate(playerPrefab);
            }

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

            while (NetworkPlayer.LocalPlayer == null)
            {
                yield return null;
            }

            ServerChangeScene(m_GameScene.SceneName);
        }
    }
    #endregion PrivateMethods
}