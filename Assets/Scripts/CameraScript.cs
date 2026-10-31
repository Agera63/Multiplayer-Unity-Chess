using Unity.Netcode;
using UnityEngine;

public class CameraScript : NetworkBehaviour
{
    public static CameraScript LocalInstance { get; private set; }

    // Which color the camera is currently placed for (null = not placed yet).
    private bool? placedForColor;
    private bool isLocalOnlinePlayer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            GetComponentInChildren<Camera>().enabled = false;
            return;
        }

        LocalInstance = this;
        isLocalOnlinePlayer = true;
        placedForColor = null;

        SyncCameraWithColor();
    }

    public override void OnNetworkDespawn()
    {
        if (LocalInstance == this)
            LocalInstance = null;
        isLocalOnlinePlayer = false;
    }

    /// <summary>
    /// Safety net: the camera must always match the player's color. Whatever order the
    /// spawn / scene load / color RPC happened in, this fixes any mismatch within one frame.
    /// </summary>
    private void LateUpdate()
    {
        if (isLocalOnlinePlayer)
            SyncCameraWithColor();
    }

    private void SyncCameraWithColor()
    {
        // Client still waiting for the host to tell it its color: don't place the camera
        // using a stale color from a previous game.
        if (NetworkGameManager.IsClientWaitingForColor) return;

        if (placedForColor != GameModeManager.instance.playerColor)
            PositionCamera();
    }

    /// <summary>
    /// During multiplayer game, it allows for both cameras to be positioned in the correct positions.
    /// </summary>
    public void PositionCamera()
    {
        Transform camTransform = GetComponentInChildren<Camera>().transform;
        bool isWhite = GameModeManager.instance.playerColor;

        if (!isWhite)
        {
            // Black: far side
            camTransform.position = new Vector3(0.5f, 5.5f, 10f);
            camTransform.rotation = Quaternion.Euler(45, 180, 0);
        }
        else
        {
            // White: near side
            camTransform.position = new Vector3(0.5f, 5.5f, -2.5f);
            camTransform.rotation = Quaternion.Euler(45, 0, 0);
        }

        placedForColor = isWhite;
    }
}