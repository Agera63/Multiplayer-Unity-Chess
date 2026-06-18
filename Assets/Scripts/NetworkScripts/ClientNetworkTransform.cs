using Unity.Netcode.Components;
using UnityEngine;

///<summary>
///Used for syncing a transform with a client side changes. This includes host, Pure Server as owner isn't supported by this.
///Please use NetworkTranform for transforms that'll always be owned by the server.
///</summary>
[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
