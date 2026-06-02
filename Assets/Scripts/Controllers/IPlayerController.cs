using System;

public interface IPlayerController
{
    bool IsHuman { get; }
    void StartTurn(Action<BoardPos> onMoveReady);
}