using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button backBtn;
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private GameObject joinSection;

    /// <summary>
    /// Initializes Unity Services and anonymous authentication if not already done,
    /// then registers the button listeners for hosting, joining, and leaving a relay session.
    /// </summary>
    private async void Start()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        hostBtn.onClick.AddListener(CreateRelay);
        joinBtn.onClick.AddListener(JoinRelay);
        backBtn.onClick.AddListener(LeaveRelay);
    }

    /// <summary>
    /// Creates a Unity Relay allocation, displays the generated join code to the host,
    /// starts the host session, and waits for a client to connect before loading the game scene.
    /// </summary>
    private async void CreateRelay()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
            await Task.Delay(500);
        }

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinSection.SetActive(false);
            codeText.text = "Code : " + joinCode;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        catch (RelayServiceException e) { Debug.Log(e); }
    }

    /// <summary>
    /// Triggered when a client connects to the host's relay session.
    /// Randomly assigns colors to both players, then loads the game scene for all connected clients.
    /// </summary>
    /// <param name="clientId">The network ID of the client that connected.</param>
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        GameModeManager.instance.SetGameMode(GameMode.PvP_Online);

        bool hostColor = Random.Range(0, 2) == 0;
        GameModeManager.instance.SetPlayerColor(hostColor);
        GameModeManager.instance.SetClientColor(!hostColor);

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    /// <summary>
    /// Triggered once all clients have finished loading the game scene.
    /// Sends the client's color assignment via RPC so they can initialize their game.
    /// </summary>
    /// <param name="sceneName">The name of the scene that was loaded.</param>
    /// <param name="mode">The load scene mode used.</param>
    /// <param name="clientsCompleted">List of client IDs that successfully loaded the scene.</param>
    /// <param name="clientsTimedOut">List of client IDs that timed out during scene loading.</param>
    private void OnSceneLoaded(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        NetworkGameManager.Instance.SetClientColorClientRpc(GameModeManager.instance.clientColor);
    }

    /// <summary>
    /// Joins an existing relay session using the join code entered by the player,
    /// sets the game mode to online, and starts the client connection.
    /// </summary>
    private async void JoinRelay()
    {
        if (codeInput.text is null || codeInput.text.Equals(string.Empty)) return;

        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
            await Task.Delay(500);
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(codeInput.text);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            GameModeManager.instance.SetGameMode(GameMode.PvP_Online);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e) { Debug.Log(e); }
    }

    /// <summary>
    /// Shuts down the current network session and resets the relay UI
    /// back to its initial state for hosting or joining a new session.
    /// </summary>
    public void LeaveRelay()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            joinSection.SetActive(true);
            codeText.text = "";
        }
    }
}