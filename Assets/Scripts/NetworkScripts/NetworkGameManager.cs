using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;
    public static bool IsClientWaitingForColor = false;
    
    public NetworkController networkController;

    public static event Action OnOpponentDisconnected;


    private void Awake()
    {
        Instance = this;

        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
            IsClientWaitingForColor = true;
    }

    public void SendMove(string moveString)
    {
        if (IsHost)
            SyncMoveClientRpc(moveString);
        else
            SyncMoveServerRpc(moveString);
    }

    [ClientRpc]
    private void SyncMoveClientRpc(string moveString)
    {
        if (IsHost) return;
        networkController?.ReceiveMove(moveString);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncMoveServerRpc(string moveString)
    {
        networkController?.ReceiveMove(moveString);
    }

    [ClientRpc]
    public void SetClientColorClientRpc(bool clientIsWhite)
    {
        if (IsHost) return;

        IsClientWaitingForColor = false;
        GameModeManager.instance.SetPlayerColor(clientIsWhite);

        if (CameraScript.LocalInstance != null)
            CameraScript.LocalInstance.PositionCamera();

        GameObject.FindGameObjectsWithTag("GameManager")[0]
            ?.GetComponent<GameManager>()
            ?.InitializeOnlineGame();
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
        IsClientWaitingForColor = false;
        OnOpponentDisconnected = null;
    }

    private void OnPlayerDisconnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            OnOpponentDisconnected?.Invoke();
    }
}