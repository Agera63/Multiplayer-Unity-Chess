using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] static GameObject[] allPieceModels;
    public event Action hideLegalMoves;

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

        PieceManager.InitializeBoard();
    }

    void Update()
    {
        //if (GameModeManager.instance.playerColor == colorTurn)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                GameObject clickedObject = ClickHelper.FindClickedObject();

                if (isValidPiece(clickedObject))
                    return;
                else if (clickedObject.TryGetComponent(out BoardTile bt) && selectedPiece is not null)
                    TryMovePiece(BoardPos.StringToPos(bt.boardPosition));
            }
        }
    }

    private void TryMovePiece(BoardPos position)
    {
        //use selectedPiece for the move
        pieceHelperScript.Move(position);
    }

    private bool isValidPiece(GameObject clickedObject)
    {
        if (pieceHelperScript is not null) pieceHelperScript.HideLegalMoves();

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
        else return false;

        return true;
    }

    public static void CreateNewPiece(PieceType type, Vector3 position, bool isWhite)
    {
        string color = isWhite ? "White" : "Black";
        string pieceName = color + " " + type.ToString();  // ex: "White Bishop"

        foreach (GameObject model in allPieceModels)
        {
            if (model.name.Equals(pieceName))
            {
                Instantiate(model, position, Quaternion.identity);
                break;
            }
        }
    }
}
