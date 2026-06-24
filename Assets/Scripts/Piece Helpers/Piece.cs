using System;
using System.Collections.Generic;
using UnityEngine;
public abstract class Piece
{
    public GameObject associatedGameObject;

    public bool isWhite;
    public BoardPos position { get; set; }
    public char icon;
    public bool isActive;

    protected static Dictionary<string, string> castlingMap;

    //resets every moves + not defiend
    public HashSet<BoardPos> legalMoves = new();

    public Piece(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject)
    {
        isWhite = _isWhite;
        position = _boardPosition;
        associatedGameObject = _associatedGameObject;

        isActive = true;
    }


    /// <summary>
    /// Moves the piece to the given board position, handling captures along the way.
    /// Also updates the board state and triggers the move animation on the associated GameObject.
    /// </summary>
    /// <param name="_boardPosition">The target position to move the piece to.</param>
    public abstract void Move(BoardPos _boardPosition);

    /// <summary>
    /// Checks if the move can be legal in any way in the current position.
    /// This validates both piece mouvement rules and king being checked.
    /// </summary>
    /// <param name="MovementChar">A char array encoding the move in the format "e2-e4", 
    /// with an optional 6th char for pawn promotion (e.g. 'q').</param>
    /// <param name="isWhiteTurn">Whether it is currently white's turn.</param>
    /// <returns>True if the move is fully legal, false otherwise.</returns>
    public static bool IsValidMove(char[] MovementChar, bool isWhiteTurn)
    {
        BoardPos finalPosition = BoardPos.StringToPos(MovementChar[3].ToString() +
                    MovementChar[4].ToString().ToLower());
        Piece PieceToMove = FindPieceAtPos(BoardPos.StringToPos(MovementChar[0].ToString() +
                MovementChar[1].ToString().ToLower()));

        try
        {
            if (PieceToMove == null || PieceToMove.isWhite != isWhiteTurn) return false;

            return IsLegalMove(PieceToMove, finalPosition);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return false;
        }
    }

    /// <summary>
    /// Checks if the position currently has a piece on it.
    /// </summary>
    /// <param name="Pos">The board position we are checking.</param>
    /// <returns>Returns a Piece if there has been 1 found, otherwise it returns null.</returns>
    public static Piece FindPieceAtPos(BoardPos Pos)
    {
        foreach (Piece p in PieceManager.AllPieces)
        {
            if (p.isActive && p.position.num == Pos.num && p.position.letter == Pos.letter)
            {
                return p;
            }
        }
        return null;
    }


    /// <summary>
    /// Validates a move by first checking the piece's movement rules, then simulating
    /// the move to ensure it does not leave or put the friendly king in check.
    /// </summary>
    /// <param name="pieceToMove">The piece attempting to move.</param>
    /// <param name="finalPosition">The target position of the move.</param>
    /// <returns>True if the move is legal and does not endanger the friendly king, false otherwise.</returns>
    public static bool IsLegalMove(Piece pieceToMove, BoardPos finalPosition)
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
            return SimulationClass.WillMoveCheckKing(pieceToMove, finalPosition);
        }
    }

    /// <summary>
    /// Checks whether a piece can move to the given position based on occupancy.
    /// </summary>
    /// <param name="p">The piece attempting to move.</param>
    /// <param name="posToMove">The target position to check.</param>
    /// <param name="condition">
    /// If true, returns false when any piece occupies the target (blocked).
    /// If false, returns false only when a friendly piece occupies the target (can still capture enemies).
    /// </param>
    /// <returns>True if the move to the target position is allowed, false otherwise.</returns>
    public bool CheckPosToMove(Piece p, BoardPos posToMove, bool condition)
    {
        foreach (Piece tempPiece in PieceManager.AllPieces)
        {
            if (!tempPiece.isActive) continue;
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

    /// <summary>
    /// Checks whether a given square is under attack by any enemy piece.
    /// </summary>
    /// <param name="square">The board position to check.</param>
    /// <param name="friendlyColor">The color of the friendly side.</param>
    /// <returns>True if the square is attacked by at least one enemy piece, false otherwise.</returns>
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


    /// <summary>
    /// Checks whether any piece is blocking the path between the current piece and the target position.
    /// </summary>
    /// <param name="finalPos">The target position to check the path towards.</param>
    /// <param name="movementType">The type of movement being performed (vertical, horizontal, or diagonal).</param>
    /// <returns>True if any piece is blocking the path, false otherwise.</returns>
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
            catch
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks whether a piece's movement to the target position follows the rules for its piece type,
    /// including direction, distance, blocking, and special moves like en passant and castling.
    /// </summary>
    /// <param name="PieceToMove">The piece attempting to move.</param>
    /// <param name="finalPosition">The target position of the move.</param>
    /// <returns>True if the movement is valid for the given piece type, false otherwise.</returns>
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
                                finalPosition.num - PieceToMove.position.num > 0 && !PieceToMove.AnyPieceBlocking(finalPosition, movementType))
                        {
                            ((PawnHelper)PieceToMove).canMove2Squares = false;
                            return true;
                        }
                        else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                                ((BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 2 &&
                                        PieceToMove.position.num == 1)) &&
                                finalPosition.num - PieceToMove.position.num > 0 && !PieceToMove.AnyPieceBlocking(finalPosition, movementType))
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
                                PieceToMove.position.num - finalPosition.num > 0 && !PieceToMove.AnyPieceBlocking(finalPosition, movementType))
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
                        // White en passant
                        else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                                BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                                && finalPosition.num - PieceToMove.position.num > 0
                                && FindPieceAtPos(finalPosition) == null)
                        {
                            Piece temppiece = FindPieceAtPos(new BoardPos(finalPosition.num - 1, finalPosition.letter));
                            if (temppiece != null
                                && temppiece.isWhite != PieceToMove.isWhite
                                && temppiece is PawnHelper
                                && ((PawnHelper)temppiece).canMove2Squares
                                && temppiece.position.num == 4)
                            {
                                // DO NOT touch temppiece or flags here — just validate
                                return true;
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
                        else if (PieceToMove.CheckPosToMove(PieceToMove, finalPosition, true) &&
                                BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                                && PieceToMove.position.num - finalPosition.num > 0
                                && FindPieceAtPos(finalPosition) == null)
                        {
                            Piece temppiece = FindPieceAtPos(new BoardPos(finalPosition.num + 1, finalPosition.letter));
                            if (temppiece != null
                                && temppiece.isWhite != PieceToMove.isWhite
                                && temppiece is PawnHelper
                                && ((PawnHelper)temppiece).canMove2Squares
                                && temppiece.position.num == 3)
                            {
                                // DO NOT touch temppiece or flags here — just validate
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
                        movementType.Equals("diagonal")) && PieceToMove is KingHelper)
                {
                    string strFinalPos = finalPosition.PosToString();
                    if (movementType.Equals("horizontal") && (strFinalPos.Equals("g1") || strFinalPos.Equals("c1") ||
                            strFinalPos.Equals("g8") || strFinalPos.Equals("c8")))
                    {
                        if (castlingMap is null) InitializeCastleMap();

                        KingHelper king = (KingHelper)PieceToMove;

                        if (king.IsChecked()) return false;

                        RookHelper r = (RookHelper)FindPieceAtPos(BoardPos.StringToPos(castlingMap[strFinalPos]));

                        if (PieceManager.CanCastle(r, king))
                        {
                            if (strFinalPos.Equals("g1") &&
                                    !PieceToMove.AnyPieceBlocking(BoardPos.StringToPos("g1"), movementType) &&
                                    !IsSquareUnderAttack(new BoardPos(0, 5), king.isWhite) &&
                                    !IsSquareUnderAttack(new BoardPos(0, 6), king.isWhite))
                            {
                                return true;
                            }
                            else if (strFinalPos.Equals("c1") &&
                                    !PieceToMove.AnyPieceBlocking(BoardPos.StringToPos("b1"), movementType) &&
                                    !IsSquareUnderAttack(new BoardPos(0, 3), king.isWhite) &&
                                    !IsSquareUnderAttack(new BoardPos(0, 2), king.isWhite))
                            {
                                return true;
                            }
                            else if (strFinalPos.Equals("g8") &&
                                    !PieceToMove.AnyPieceBlocking(BoardPos.StringToPos("g8"), movementType) &&
                                    !IsSquareUnderAttack(new BoardPos(7, 5), king.isWhite) &&
                                    !IsSquareUnderAttack(new BoardPos(7, 6), king.isWhite))
                            {
                                return true;
                            }
                            else if (strFinalPos.Equals("c8") &&
                                    !PieceToMove.AnyPieceBlocking(BoardPos.StringToPos("b8"), movementType) &&
                                    !IsSquareUnderAttack(new BoardPos(7, 3), king.isWhite) &&
                                    !IsSquareUnderAttack(new BoardPos(7, 2), king.isWhite))
                            {
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
                    Piece targetPiece = FindPieceAtPos(finalPosition);
                    if (targetPiece != null && targetPiece.isWhite != PieceToMove.isWhite)
                    {
                        return true;
                    }
                    else if (targetPiece == null)
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
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Initializes the castling map that links a king's castling destination square
    /// to the rook's starting square. Only initializes once if not already set.
    /// </summary>
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

    /// <summary>
    /// Computes and stores all legal moves for this piece by iterating over every square
    /// on the board and calling <see cref="IsLegalMove"/> on each one.
    /// Results are stored in <see cref="legalMoves"/> for use in move highlighting.
    /// </summary>
    public void SetLegalMoves()
    {
        for (int number = 0; number < 8; number++)
        {
            for (int letter = 0; letter < 8; letter++)
            {
                BoardPos boardPositionToCheck = new BoardPos(number, letter);
                if (IsLegalMove(this, boardPositionToCheck))
                {
                    legalMoves.Add(boardPositionToCheck);
                }
            }
        }
    }

    /// <summary>
    /// Resets the color of all board tiles that were highlighted in blue back to their
    /// original color (white or black) based on the tile's <see cref="BoardTile.isWhite"/> property.
    /// </summary>
    public void HideLegalMoves()
    {
        foreach (GameObject go in BoardPos.GameTiles)
        {
            var boardTileScript = go.GetComponent<BoardTile>();
            var meshRender = go.GetComponent<MeshRenderer>();

            if (meshRender.material.color == Color.blue)
            {
                if (boardTileScript.isWhite)
                {
                    meshRender.material.color = Color.white;
                }
                else
                {
                    meshRender.material.color = Color.black;
                }
            }
        }
    }
}