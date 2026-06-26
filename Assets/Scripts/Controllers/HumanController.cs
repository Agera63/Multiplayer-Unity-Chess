using System;

public class HumanController : IPlayerController
{
    public bool IsHuman => true;
    private Action<string> onMoveReady;

    public void StartTurn(Action<string> onMoveReady)
    {
        this.onMoveReady = onMoveReady;
    }

    /// <summary>
    /// Attempts to make a move on the board.
    /// </summary>
    /// <param name="moveString">The move formated in the "e2-e4" format.</param>
    public void MakeMove(string moveString)
    {
        onMoveReady?.Invoke(moveString);
    }
}