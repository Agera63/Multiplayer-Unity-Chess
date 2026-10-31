using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;
    public static bool IsClientWaitingForColor = false;

    /// <summary>True when the local player is leaving on purpose (Exit button), so our own disconnect isn't treated as the opponent leaving.</summary>
    public static bool LeavingOnPurpose = false;

    public NetworkController networkController;
    public static event Action OnOpponentDisconnected;

    private void Awake()
    {
        Instance = this;
        LeavingOnPurpose = false;

        // Assign (not just set-to-true) so a stale value from a previous game can never stick.
        IsClientWaitingForColor = NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;
    }

    /// <summary>
    /// Sends the local player's move to the opponent over the network,
    /// using a ClientRpc if the sender is the host, or a ServerRpc otherwise.
    /// </summary>
    /// <param name="moveString">The move to send in the format "e2-e4".</param>
    public void SendMove(string moveString)
    {
        if (IsHost)
            SyncMoveClientRpc(moveString);
        else
            SyncMoveServerRpc(moveString);
    }

    /// <summary>
    /// Receives a move from the host and forwards it to the <see cref="NetworkController"/>
    /// to be processed as the opponent's move on the client side.
    /// </summary>
    /// <param name="moveString">The move received from the host in the format "e2-e4".</param>
    [ClientRpc]
    private void SyncMoveClientRpc(string moveString)
    {
        if (IsHost) return;
        networkController?.ReceiveMove(moveString);
    }

    /// <summary>
    /// Receives a move from the client and forwards it to the <see cref="NetworkController"/>
    /// to be processed as the opponent's move on the host side.
    /// </summary>
    /// <param name="moveString">The move received from the client in the format "e2-e4".</param>
    [ServerRpc(RequireOwnership = false)]
    private void SyncMoveServerRpc(string moveString)
    {
        networkController?.ReceiveMove(moveString);
    }

    /// <summary>
    /// Assigns the client's color, repositions their camera, and initializes
    /// the online game on the client side once the scene has fully loaded.
    /// </summary>
    /// <param name="clientIsWhite">True if the client is assigned the white pieces, false for black.</param>
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

    /// <summary>
    /// Subscribes to the client disconnect callback when this object spawns on the network.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
    }

    /// <summary>
    /// Unsubscribes from the client disconnect callback and resets network state
    /// when this object is removed from the network.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
        IsClientWaitingForColor = false;

        // Backup: if the network is shutting down and we didn't cause it, the opponent is gone.
        if (!LeavingOnPurpose && NetworkManager.Singleton.ShutdownInProgress)
            OnOpponentDisconnected?.Invoke();

        OnOpponentDisconnected = null;
    }

    /// <summary>
    /// Fires <see cref="OnOpponentDisconnected"/> when a player other than the local client disconnects.
    /// </summary>
    /// <param name="clientId">The network ID of the client that disconnected.</param>
    private void OnPlayerDisconnected(ulong clientId)
    {
        // We pressed Exit ourselves: not the opponent's doing.
        if (LeavingOnPurpose) return;

        // Host: another client left, so clientId is the opponent's.
        // Client: Netcode only reports our OWN id when the host goes away,
        // so any disconnect we didn't cause means the host left.
        if (NetworkManager.Singleton.IsHost)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
                OnOpponentDisconnected?.Invoke();
        }
        else
        {
            OnOpponentDisconnected?.Invoke();
        }
    }
}