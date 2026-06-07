using ChessClient;
using System;
using System.Collections;
using UnityEngine;

public class StockfishController : IPlayerController
{
    public bool IsHuman => false;
    public bool isWhite;

    private MonoBehaviour coroutineRunner;

    // Needs a MonoBehaviour to run coroutines since this isn't one
    public StockfishController(MonoBehaviour runner, bool _isWhite)
    {
        isWhite = _isWhite;
        coroutineRunner = runner;
    }

    public void StartTurn(Action<BoardPos> onMoveReady)
    {
        coroutineRunner.StartCoroutine(GetStockfishMove(onMoveReady));
    }

    IEnumerator GetStockfishMove(Action<BoardPos> onMoveReady)
    {
        yield return new WaitForSeconds(1f);

        // Get move from Stockfish (your existing client)
        string move = StockFishChessClient.GetBestMoveFromBoard(PieceManager.GetBoard());

        // Convert move string to BoardPos and fire callback
        BoardPos targetPos = BoardPos.StringToPos(move.Substring(3, 2));
        onMoveReady?.Invoke(targetPos);
    }
}
