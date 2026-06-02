using System;
using UnityEngine;

public class HumanController : IPlayerController
{
    public bool IsHuman => true;
    private Action<BoardPos> onMoveReady;

    public void StartTurn(Action<BoardPos> onMoveReady)
    {
        throw new NotImplementedException();
    }

    public void MakeMove(BoardPos position)
    {
        // Called by GameManager when player clicks a valid destination
        onMoveReady?.Invoke(position);
    }
}
