using Unity.Netcode;
using UnityEngine;

public class CameraScript : NetworkBehaviour
{
    public static CameraScript LocalInstance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            GetComponentInChildren<Camera>().enabled = false;
            return;
        }

        LocalInstance = this;

        if (!NetworkGameManager.IsClientWaitingForColor)
            PositionCamera();
    }

    /// <summary>
    /// During multiplayer game, it allows for both cameras to be positioned in the correct positions.
    /// </summary>
    public void PositionCamera()
    {
        Transform camTransform = GetComponentInChildren<Camera>().transform;

        if (!GameModeManager.instance.playerColor)
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
    }
}