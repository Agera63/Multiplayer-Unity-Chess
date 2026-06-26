using System;

public interface IPlayerController
{
    bool IsHuman { get; }
    /// <summary>
    /// Signals the current controller that it is their turn to make a move.
    /// The controller will then call <paramref name="onMoveReady"/> once the move
    /// is decided passing it in the "e2-e4" format.
    /// </summary>
    /// <param name="onMoveReady">Callback to invoke when the move is ready.</param>
    void StartTurn(Action<string> onMoveReady);
}