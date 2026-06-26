using System;

public class NetworkController : IPlayerController
{
    public bool IsHuman => false;
    private Action<string> onMoveReady;

    public void StartTurn(Action<string> onMoveReady)
    {
        // Store the callback and wait
        // exactly like HumanController
        // but instead of waiting for a click,
        // we wait for a network message to arrive
        this.onMoveReady = onMoveReady;
    }

    /// <summary>
    /// Allows for a move to be received through the network.
    /// </summary>
    /// <param name="moveString">The move in the "e2-e4" format.</param>
    public void ReceiveMove(string moveString)
    {
        onMoveReady?.Invoke(moveString);
    }
}
