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

    // This gets called when a move arrives from the opponent over the network
    public void ReceiveMove(string moveString)
    {
        onMoveReady?.Invoke(moveString);
    }
}
