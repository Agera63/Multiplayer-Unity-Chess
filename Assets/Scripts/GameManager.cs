using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private bool colorTurn; //True = white turn
    private GameObject selectedPiece;
    private Piece pieceHelperScript;

    private IPlayerController whiteController;
    private IPlayerController blackController;
    private IPlayerController currentController;


    void Start()
    {
        //White always starts
        colorTurn = true;
        selectedPiece = null;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetSelectedPiece();
        }
    }

    private void TryMovePiece(BoardPos position)
    {
        //use selectedPiece for the move
        pieceHelperScript.Move(new BoardPos(1, 2));
    }

    private void SetSelectedPiece()
    {
        //Stores clicked object
        GameObject clickedObject = ClickHelper.FindClickedObject();

        //Checks its a chess piece
        if (clickedObject.TryGetComponent(out Pawn pawn))
        {
            selectedPiece = clickedObject;
            pieceHelperScript = pawn.helperClass;
            pawn.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Bishop bishop))
        {
            selectedPiece = clickedObject;
            pieceHelperScript = bishop.helperClass;
            bishop.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out King king))
        {
            selectedPiece = clickedObject;
            pieceHelperScript = king.helperClass;
            king.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Queen queen))
        {
            selectedPiece = clickedObject;
            pieceHelperScript = queen.helperClass;
            queen.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Knight knight))
        {
            selectedPiece = clickedObject;
            pieceHelperScript = knight.helperClass;
            knight.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Rook rook))
        {
            selectedPiece = clickedObject;
            pieceHelperScript = rook.helperClass;
            rook.ShowLegalMoves();
        }
        else return;
    }
}
