using ChessClient;
using System;
using System.Collections;
using UnityEngine;

public class StockfishController : IPlayerController
{
    public bool IsHuman => false;
    public bool isWhite;
    private MonoBehaviour coroutineRunner;
    private Action<string> onMoveReady;

    public StockfishController(MonoBehaviour runner, bool _isWhite)
    {
        isWhite = _isWhite;
        coroutineRunner = runner;
    }

    public void StartTurn(Action<string> onMoveReady)
    {
        this.onMoveReady = onMoveReady;
        coroutineRunner.StartCoroutine(GetStockfishMove());
    }

    IEnumerator GetStockfishMove()
    {
        yield return new WaitForSeconds(1f);
        string move = StockFishChessClient.GetBestMoveFromBoard(PieceManager.GetBoard());
        onMoveReady?.Invoke(move);
    }
}