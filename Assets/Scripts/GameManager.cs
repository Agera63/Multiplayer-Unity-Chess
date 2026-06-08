using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject[] allPieceModels;

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
        PieceManager.removeGameObj += DestroyPiece;
    }

    void Update()
    {
        if (GameModeManager.instance.playerColor == colorTurn)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                GameObject clickedObject = ClickHelper.FindClickedObject();

                if (isValidPiece(clickedObject))
                    return;
                
                //When a piece is selected and has legalMoves available, you enter the if
                if (selectedPiece is not null && pieceHelperScript.legalMoves.Count != 0)
                {
                    //if you clicked a board tile, move to its position
                    if (clickedObject.TryGetComponent(out BoardTile bt))
                        TryMovePiece(BoardPos.StringToPos(bt.boardPosition));
                    //if you clicked on a piece of different color, see if that move is legal, then move
                    else if (clickedObject.TryGetComponent(out MonoBehaviorPiece MBp) 
                        && MBp.isWhite != GameModeManager.instance.playerColor)
                        TryMovePiece(MBp.helperClass.position);
                }
            }
        }
    }

    private void TryMovePiece(BoardPos position)
    {
        char[] mouvementChar = (pieceHelperScript.position.PosToString() + "-" + position.PosToString()).ToCharArray();
        if (Piece.IsValidMove(mouvementChar)) 
        { 
            PieceManager.Update(mouvementChar);
            colorTurn = !colorTurn;

            //Resets selected piece
            selectedPiece = null;
            pieceHelperScript = null;
        }
    }

    private bool isValidPiece(GameObject clickedObject)
    {
        if (pieceHelperScript is not null) pieceHelperScript.HideLegalMoves();

        //Checks its a chess piece
        if (clickedObject.TryGetComponent(out Pawn pawn) 
            && pawn.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = pawn.helperClass;
            pawn.ShowLegalMoves();
            
            //Checks if the promotion action event has the creation method assigned
            if (!((PawnHelper) pawn.helperClass).CheckPromotionActions(CreateNewPiece)) ((PawnHelper) pawn.helperClass).promote += CreateNewPiece;
        }
        else if (clickedObject.TryGetComponent(out Bishop bishop) 
            && bishop.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = bishop.helperClass;
            bishop.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out King king)
            && king.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = king.helperClass;
            king.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Queen queen)
            && queen.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = queen.helperClass;
            queen.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Knight knight)
            && knight.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = knight.helperClass;
            knight.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Rook rook)
            && rook.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = rook.helperClass;
            rook.ShowLegalMoves();
        }
        else return false;

        return true;
    }

    //Add sound for every Destroyed piece
    private void DestroyPiece(Piece pieceToRemove) => Destroy(pieceToRemove.associatedGameObject);

    private void CreateNewPiece(PieceType type, Vector3 position, bool isWhite)
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
