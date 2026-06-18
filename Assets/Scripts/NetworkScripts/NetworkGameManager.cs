using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;
    public static bool IsClientWaitingForColor = false;

    public NetworkController networkController;

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

        CameraScript.LocalInstance?.PositionCamera();

        // Now that color is known, initialize the game controllers on the client
        GameObject.FindGameObjectsWithTag("GameManager")[0]?.GetComponent<GameManager>()?.InitializeOnlineGame();
    }
}