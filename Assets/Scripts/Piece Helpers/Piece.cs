using System;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;
public abstract class Piece
{
    public GameObject associatedGameObject;

    public bool isWhite;
    public BoardPos position { get; set; }
    public char icon;
    public bool isActive;

    private static Dictionary<string, string> castlingMap;

    //resets every moves + not defiend
    public HashSet<BoardPos> legalMoves = new();

    public Piece(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject)
    {
        isWhite = _isWhite;
        position = _boardPosition;
        associatedGameObject = _associatedGameObject;

        isActive = true;
    }

    public abstract void Move(BoardPos _boardPosition);

    public abstract bool IsValidMove(BoardPos start, BoardPos end);

    public static Piece FindPieceAtPos(BoardPos Pos)
    {
        foreach (Piece p in PieceManager.AllPieces)
        {
            if (p.position.num == Pos.num && p.position.letter == Pos.letter)
            {
                return p;
            }
        }
        return null;
    }

    public static bool IsValidMoveWithCheckValidation(Piece pieceToMove, BoardPos finalPosition)
    {
        if (!CheckPieceMovement(pieceToMove, finalPosition))
        {
            return false;
        }

        KingHelper kingToCheck = pieceToMove.isWhite ? KingHelper.FindWhiteKing() : KingHelper.FindBlackKing();

        if (kingToCheck == null)
        {
            return false;
        }

        bool kingChecked = kingToCheck.IsChecked();

        if (kingChecked)
        {
            return SimulationClass.KingSim(pieceToMove, finalPosition, kingToCheck);
        }
        else
        {
            if (pieceToMove is King)
            {
                return SimulationClass.WillMoveCheckKing(pieceToMove, finalPosition);
            }
            else
            {
                return SimulationClass.WillMoveCheckKing(pieceToMove, finalPosition);
            }
        }
    }

    public bool CheckPosToMove(Piece p, BoardPos posToMove, bool condition)
    {
        foreach (Piece tempPiece in PieceManager.AllPieces)
        {
            if (condition)
            {
                if (posToMove.num == tempPiece.position.num && posToMove.letter == tempPiece.position.letter)
                {
                    return false;
                }
            }
            else
            {
                if (posToMove.num == tempPiece.position.num && posToMove.letter == tempPiece.position.letter
                        && p.isWhite != tempPiece.isWhite)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static bool IsSquareUnderAttack(BoardPos square, bool friendlyColor)
    {
        foreach (Piece enemyPiece in PieceManager.AllPieces)
        {
            if (enemyPiece.isWhite != friendlyColor && enemyPiece.isActive)
            {
                string movementType = BoardPos.CheckMovementDirection(enemyPiece.position, square);

                if (movementType == null)
                {
                    continue;
                }

                if (enemyPiece is PawnHelper)
                {
                    if (movementType.Equals("diagonal") &&
                       BoardPos.SquaresMoved(movementType, enemyPiece.position, square) == 1)
                    {
                        if (enemyPiece.isWhite)
                        {
                            if (square.num - enemyPiece.position.num > 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (enemyPiece.position.num - square.num > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (enemyPiece is KnightHelper)
                {
                    if (movementType.Equals("knight"))
                    {
                        return true;
                    }
                }
                else if (enemyPiece is BishopHelper)
                {
                    if (movementType.Equals("diagonal") &&
                       !enemyPiece.AnyPieceBlocking(square, movementType))
                    {
                        return true;
                    }
                }
                else if (enemyPiece is RookHelper)
                {
                    if ((movementType.Equals("vertical") || movementType.Equals("horizontal")) &&
                       !enemyPiece.AnyPieceBlocking(square, movementType))
                    {
                        return true;
                    }
                }
                else if (enemyPiece is QueenHelper)
                {
                    if ((movementType.Equals("vertical") || movementType.Equals("horizontal") ||
                        movementType.Equals("diagonal")) &&
                       !enemyPiece.AnyPieceBlocking(square, movementType))
                    {
                        return true;
                    }
                }
                else if (enemyPiece is KingHelper)
                {
                    if (BoardPos.SquaresMoved(movementType, enemyPiece.position, square) == 1)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool AnyPieceBlocking(BoardPos finalPos, string movementType)
    {
        int slotsToCheck = BoardPos.SquaresMoved(movementType, this.position, finalPos);
        for (int i = 1; i < slotsToCheck; i++)
        {
            try
            {
                if (movementType.Equals("vertical"))
                {
                    if (this.position.num < finalPos.num)
                    {
                        if (PieceManager.GetBoard()[this.position.num + i, this.position.letter] != '\0')
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (PieceManager.GetBoard()[this.position.num - i, this.position.letter] != '\0')
                        {
                            return true;
                        }
                    }
                }
                else if (movementType.Equals("horizontal"))
                {
                    if (this.position.letter < finalPos.letter)
                    {
                        if (PieceManager.GetBoard()[this.position.num, this.position.letter + i] != '\0')
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (PieceManager.GetBoard()[this.position.num, this.position.letter - i] != '\0')
                        {
                            return true;
                        }
                    }
                }
                else if (movementType.Equals("diagonal"))
                {
                    if (this.position.num < finalPos.num && this.position.letter < finalPos.letter)
                    {
                        if (PieceManager.GetBoard()[this.position.num + i, this.position.letter + i] != '\0')
                        {
                            if (finalPos.PosToString().Equals(new BoardPos(this.position.num + i, this.position.letter + i).PosToString()))
                            {
                                if (FindPieceAtPos(new BoardPos(this.position.num + i, this.position.letter + i)).isWhite == this.isWhite)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    else if (this.position.num > finalPos.num && this.position.letter < finalPos.letter)
                    {
                        if (PieceManager.GetBoard()[this.position.num - i, this.position.letter + i] != '\0')
                        {
                            if (finalPos.PosToString().Equals(new BoardPos(this.position.num - i, this.position.letter + i).PosToString()))
                            {
                                if (FindPieceAtPos(new BoardPos(this.position.num - i, this.position.letter + i)).isWhite == this.isWhite)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    else if (this.position.num < finalPos.num && this.position.letter > finalPos.letter)
                    {
                        if (PieceManager.GetBoard()[this.position.num + i, this.position.letter - i] != '\0')
                        {
                            if (finalPos.PosToString().Equals(new BoardPos(this.position.num + i, this.position.letter - i).PosToString()))
                            {
                                if (FindPieceAtPos(new BoardPos(this.position.num + i, this.position.letter - i)).isWhite == this.isWhite)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    else if (this.position.num > finalPos.num && this.position.letter > finalPos.letter)
                    {
                        if (PieceManager.GetBoard()[this.position.num - i, this.position.letter - i] != '\0')
                        {
                            if (finalPos.PosToString().Equals(new BoardPos(this.position.num - i, this.position.letter - i).PosToString()))
                            {
                                if (FindPieceAtPos(new BoardPos(this.position.num - i, this.position.letter - i)).isWhite == this.isWhite)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return true;
            }
        }
        return false;
    }

    public static bool CheckPieceMovement(Piece PieceToMove, BoardPos finalPosition)
    {
        try
        {
            string movementType = BoardPos.CheckMovementDirection(PieceToMove.position, finalPosition);
            if (movementType != null)
            {
                if (movementType.Equals("vertical") && PieceToMove is PawnHelper)
                {
                    if (PieceToMove.isWhite)
                    {
                        if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                                (BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1) &&
                                finalPosition.num - PieceToMove.position.num > 0)
                        {
                            ((PawnHelper)PieceToMove).canMove2Squares = false;
                            return true;
                        }
                        else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                                ((BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 2 &&
                                        PieceToMove.position.num == 1)) &&
                                finalPosition.num - PieceToMove.position.num > 0)
                        {
                            ((PawnHelper)PieceToMove).canMove2Squares = true;
                            return true;
                        }
                    }
                    else
                    {
                        if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                                (BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1) &&
                                PieceToMove.position.num - finalPosition.num > 0)
                        {
                            ((PawnHelper)PieceToMove).canMove2Squares = false;
                            return true;
                        }
                        else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                                ((BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 2 &&
                                        PieceToMove.position.num == 6)) &&
                                PieceToMove.position.num - finalPosition.num > 0)
                        {
                            ((PawnHelper)PieceToMove).canMove2Squares = true;
                            return true;
                        }
                    }
                }
                else if (movementType.Equals("diagonal") && PieceToMove is PawnHelper)
                {
                    if (PieceToMove.isWhite)
                    {
                        if (!PieceToMove.CheckPosToMove(PieceToMove, finalPosition, false) &&
                                BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                                && finalPosition.num - PieceToMove.position.num > 0
                                && FindPieceAtPos(finalPosition) != null)
                        {
                            if (FindPieceAtPos(finalPosition).isWhite == !PieceToMove.isWhite)
                            {
                                ((PawnHelper)PieceToMove).canMove2Squares = false;
                                return true;
                            }
                        }
                        else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                                BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                                && finalPosition.num - PieceToMove.position.num > 0
                                && FindPieceAtPos(finalPosition) == null)
                        {
                            Piece temppiece = FindPieceAtPos(new BoardPos(finalPosition.num - 1, finalPosition.letter));
                            if (temppiece != null)
                            {
                                if (temppiece.isWhite != PieceToMove.isWhite && temppiece is PawnHelper &&
                                    ((PawnHelper)temppiece).canMove2Squares && temppiece.position.num == 4)
                                {
                                    ((PawnHelper)temppiece).canMove2Squares = false;
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!PieceToMove.CheckPosToMove(PieceToMove, finalPosition, false) &&
                                BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                                && PieceToMove.position.num - finalPosition.num > 0
                                && FindPieceAtPos(finalPosition) != null)
                        {
                            if (FindPieceAtPos(finalPosition).isWhite == !PieceToMove.isWhite)
                            {
                                ((PawnHelper)PieceToMove).canMove2Squares = false;
                                return true;
                            }
                        }
                        else if (!PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                                BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                                && PieceToMove.position.num - finalPosition.num > 0
                                && FindPieceAtPos(finalPosition) == null)
                        {
                            Piece temppiece = FindPieceAtPos(new BoardPos(finalPosition.num - 1, finalPosition.letter));
                            if (temppiece.isWhite != PieceToMove.isWhite && temppiece is PawnHelper &&
                                    ((PawnHelper)temppiece).canMove2Squares && temppiece.position.num == 3)
                            {
                                ((PawnHelper)temppiece).canMove2Squares = false;
                                return true;
                            }
                        }
                    }
                }
                else if ((movementType.Equals("vertical") || movementType.Equals("horizontal")) &&
                        PieceToMove is RookHelper)
                {
                    if (!PieceToMove.CheckPosToMove(PieceToMove, finalPosition, false) &&
                            !PieceToMove.AnyPieceBlocking(finalPosition, movementType))
                    {
                        return true;
                    }
                    else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                            !PieceToMove.AnyPieceBlocking(finalPosition, movementType))
                    {
                        return true;
                    }
                }
                else if ((movementType.Equals("vertical") || movementType.Equals("horizontal")
                        || movementType.Equals("diagonal")) && PieceToMove is QueenHelper)
                {
                    if (!PieceToMove.CheckPosToMove(PieceToMove, finalPosition, false) &&
                            !PieceToMove.AnyPieceBlocking(finalPosition, movementType))
                    {
                        return true;
                    }
                    else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                            !PieceToMove.AnyPieceBlocking(finalPosition, movementType))
                    {
                        return true;
                    }
                }
                else if ((movementType.Equals("vertical") || movementType.Equals("horizontal") ||
                        movementType.Equals("diagonal") && PieceToMove is KingHelper))
                {
                    string strFinalPos = finalPosition.PosToString();
                    if (movementType.Equals("horizontal") && (strFinalPos.Equals("g1") || strFinalPos.Equals("c1") ||
                            strFinalPos.Equals("g8") || strFinalPos.Equals("c8")))
                    {
                        if (castlingMap is null) InitializeCastleMap();

                        KingHelper king = (KingHelper)PieceToMove;

                        // CRITICAL: Cannot castle if king is in check
                        if (king.IsChecked())
                        {
                            return false;
                        }

                        RookHelper r = (RookHelper)FindPieceAtPos(BoardPos.StringToPos(castlingMap[strFinalPos]));
                        if (PieceManager.CanCastle(r, king))
                        {
                            // Check if path is clear AND king doesn't pass through check
                            if (strFinalPos.Equals("g1") &&
                                    !PieceToMove.AnyPieceBlocking(BoardPos.StringToPos("g1"), movementType) &&
                                    !IsSquareUnderAttack(new BoardPos(0, 5), king.isWhite) && // f1
                                    !IsSquareUnderAttack(new BoardPos(0, 6), king.isWhite))
                            {  // g1
                                return true;
                            }
                            else if (strFinalPos.Equals("c1") &&
                                    !PieceToMove.AnyPieceBlocking(BoardPos.StringToPos("b1"), movementType) &&
                                    !IsSquareUnderAttack(new BoardPos(0, 3), king.isWhite) && // d1
                                    !IsSquareUnderAttack(new BoardPos(0, 2), king.isWhite))
                            {  // c1
                                return true;
                            }
                            else if (strFinalPos.Equals("g8") &&
                                    !PieceToMove.AnyPieceBlocking(BoardPos.StringToPos("g8"), movementType) &&
                                    !IsSquareUnderAttack(new BoardPos(7, 5), king.isWhite) && // f8
                                    !IsSquareUnderAttack(new BoardPos(7, 6), king.isWhite))
                            {  // g8
                                return true;
                            }
                            else if (strFinalPos.Equals("c8") &&
                                    !PieceToMove.AnyPieceBlocking(BoardPos.StringToPos("b8"), movementType) &&
                                    !IsSquareUnderAttack(new BoardPos(7, 3), king.isWhite) && // d8
                                    !IsSquareUnderAttack(new BoardPos(7, 2), king.isWhite))
                            {  // c8
                                return true;
                            }
                        }
                    }
                    if (!PieceToMove.CheckPosToMove(PieceToMove, finalPosition, false) &&
                            !PieceToMove.AnyPieceBlocking(finalPosition, movementType) &&
                            BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1)
                    {
                        return true;
                    }
                    else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                            !PieceToMove.AnyPieceBlocking(finalPosition, movementType) &&
                            BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1)
                    {
                        return true;
                    }
                }
                else if (movementType.Equals("knight") && PieceToMove is KnightHelper)
                {
                    if (!PieceToMove.CheckPosToMove(PieceToMove, finalPosition, false) &&
                            FindPieceAtPos(finalPosition) != null &&
                            FindPieceAtPos(finalPosition).isWhite == !PieceToMove.isWhite)
                    {
                        return true;
                    }
                    else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true))
                    {
                        return true;
                    }
                }
                else if (movementType.Equals("diagonal") && PieceToMove is BishopHelper)
                {
                    if (!PieceToMove.CheckPosToMove(PieceToMove, finalPosition, false) &&
                            !PieceToMove.AnyPieceBlocking(finalPosition, movementType))
                    {
                        return true;
                    }
                    else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                            !PieceToMove.AnyPieceBlocking(finalPosition, movementType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static void InitializeCastleMap()
    {
        if (castlingMap == null)
        {
            castlingMap = new Dictionary<string, string>();
            castlingMap.Add("g1", "h1");
            castlingMap.Add("c1", "a1");
            castlingMap.Add("g8", "h8");
            castlingMap.Add("c8", "a8");
        }
    }

    //Code for unity visuals 
    public void SetLegalMoves()
    {
        for (int number = 0; number < 8; number++)
        {
            for (int letter = 0; letter < 8; letter++)
            {
                BoardPos boardPositionToCheck = new BoardPos(number, letter);
                if (CheckPieceMovement(this, boardPositionToCheck))
                {
                    legalMoves.Add(boardPositionToCheck);
                }
            }
        }
    }

    public void HideLegalMoves()
    {
        foreach(GameObject go in BoardPos.GameTiles)
        {
            var boardTileScript = go.GetComponent<BoardTile>(); 
            var meshRender = go.GetComponent<MeshRenderer>();
            
            if(meshRender.material.color == Color.blue)
            {
                if (boardTileScript.isWhite)
                {
                    meshRender.material.color = Color.white;
                } else
                {
                    meshRender.material.color = Color.black;
                }
            }
        }
    }
}



