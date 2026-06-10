using System;

public class HumanController : IPlayerController
{
    public bool IsHuman => true;
    private Action<string> onMoveReady;

    public void StartTurn(Action<string> onMoveReady)
    {
        this.onMoveReady = onMoveReady;
    }

    public void MakeMove(string moveString)
    {
        onMoveReady?.Invoke(moveString);
    }
}