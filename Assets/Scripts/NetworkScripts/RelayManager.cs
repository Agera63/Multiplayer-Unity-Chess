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

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        NetworkGameManager.Instance.SetClientColorClientRpc(GameModeManager.instance.clientColor);
    }

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

            // FIX: set PvP_Online BEFORE StartClient() so that when the scene
            // loads and GameManager.Start() runs, AssignControllers() sees the
            // correct mode and creates the NetworkController
            GameModeManager.instance.SetGameMode(GameMode.PvP_Online);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e) { Debug.Log(e); }
    }

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