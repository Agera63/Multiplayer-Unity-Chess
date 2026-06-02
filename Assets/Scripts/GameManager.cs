using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool colorTurn; //True = white turn
    private GameObject selectedPiece;

    void Start()
    {
        colorTurn = true;
        selectedPiece = null;
    }

    void Update()
    {
        if (selectedPiece == null) return;
    }

    private void TryMovePiece(BoardPos position)
    {
        //use selectedPiece for the move
    }
}
