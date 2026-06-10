using System;

public interface IPlayerController
{
    bool IsHuman { get; }
    // CHANGE: Action now takes a string instead of BoardPos
    void StartTurn(Action<string> onMoveReady);
}