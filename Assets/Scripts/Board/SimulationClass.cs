using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class SimulationClass
{
    private char[,] BoardCopy;
    private List<Piece> GOCopy;
    private KingHelper KingToMove;

    private SimulationClass(KingHelper k)
    {
        BoardCopy = DeepCopyBoard(PieceManager.GetBoard());
        GOCopy = DeepCopyPieces();
        KingToMove = FindKing(k);
    }

    public static bool KingSim(Piece pieceToMove, BoardPos finalPos, KingHelper k)
    {
        SimulationClass sc = new SimulationClass(k);
        Piece simPiece = sc.FindPieceOfPosSim(pieceToMove.position);

        if (simPiece == null)
        {
            return false;
        }

        char[] movementCharSim = (pieceToMove.position.PosToString() + "-" + finalPos.PosToString()).ToCharArray();
        sc.UpdateSim(movementCharSim);

        return !sc.IsKingCheckedSim(sc.KingToMove);
    }

    public static bool WillMoveCheckKing(Piece pieceToMove, BoardPos finalPos)
    {
        KingHelper kingToCheck;
        if (pieceToMove is KingHelper)
        {
            kingToCheck = (KingHelper) pieceToMove;
        }
        else
        {
            kingToCheck = pieceToMove.isWhite ? KingHelper.FindWhiteKing() : KingHelper.FindBlackKing();
        }

        if (kingToCheck == null)
        {
            return false;
        }

        SimulationClass sc = new SimulationClass(kingToCheck);
        Piece simPiece = sc.FindPieceOfPosSim(pieceToMove.position);

        if (simPiece == null)
        {
            return false;
        }

        char[] movementCharSim = (pieceToMove.position.PosToString() + "-" + finalPos.PosToString()).ToCharArray();
        sc.UpdateSim(movementCharSim);

        return !sc.IsKingCheckedSim(sc.KingToMove);
    }

    public static bool IsCheckMate(KingHelper k)
    {
        if (!k.IsChecked())
        {
            return false;
        }

        foreach (Piece p in PieceManager.AllPieces)
        {
            if (p.isWhite == k.isWhite && p.isActive)
            {
                for (int num = 0; num < 8; num++)
                {
                    for (int letter = 0; letter < 8; letter++)
                    {
                        BoardPos targetPos = new BoardPos(num, letter);

                        if (Piece.CheckPieceMovement(p, targetPos))
                        {
                            if (KingSim(p, targetPos, k))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    private KingHelper FindKing(KingHelper K)
    {
        foreach (Piece p in GOCopy)
        {
            if (p.position.letter == K.position.letter && p.position.num == K.position.num && p is King)
            {
                return (KingHelper)p;
            }
        }
        return null;
    }

    private void UpdateSim(char[] MovementChar)
    {
        string PieceToMove = MovementChar[0].ToString() + MovementChar[1].ToString().ToLower();
        string PositionToMove = MovementChar[3].ToString() + MovementChar[4].ToString().ToLower();

        foreach (Piece p in GOCopy)
        {
            if (p.position.PosToString().Equals(PieceToMove.ToLower()))
            {
                MovementSim(BoardPos.StringToPos(PositionToMove.ToLower()), p);
                if (MovementChar.Length == 6 && p is PawnHelper)
                {
                    if ((p.position.num == 0 && !GameModeManager.instance.playerColor) || (p.position.num == 7 && GameModeManager.instance.playerColor))
                    {
                        bool temp = true;
                        do
                        {
                            switch (MovementChar[5])
                            {
                                case 'n':
                                    KnightHelper n = new KnightHelper((KnightHelper)((PawnHelper)p).PromotionSimulation(MovementChar[5]));
                                    temp = false;
                                    GOCopy.Add(n);
                                    break;
                                case 'q':
                                    QueenHelper q = new QueenHelper((QueenHelper)((PawnHelper)p).PromotionSimulation(MovementChar[5]));
                                    temp = false;
                                    GOCopy.Add(q);
                                    break;
                                case 'r':
                                    RookHelper r = new RookHelper((RookHelper)((PawnHelper)p).PromotionSimulation(MovementChar[5]));
                                    temp = false;
                                    GOCopy.Add(r);
                                    break;
                                case 'b':
                                    BishopHelper b = new BishopHelper((BishopHelper)((PawnHelper)p).PromotionSimulation(MovementChar[5]));
                                    temp = false;
                                    GOCopy.Add(b);
                                    break;
                            }
                        } while (temp);
                        p.isActive = false;
                    }
                }
                break;
            }
        }

        for (int i = GOCopy.Count - 1; i >= 0; i--)
        {
            if (!GOCopy[i].isActive)
            {
                GOCopy.RemoveAt(i);
            }
        }
    }

    private void MovementSim(BoardPos targetPos, Piece PieceToMove)
    {
        Piece targetPiece = FindPieceOfPosSim(targetPos);
        if (targetPiece != null && targetPiece.isWhite != PieceToMove.isWhite)
        {
            targetPiece.isActive = false;
        }

        BoardCopy[PieceToMove.position.num, PieceToMove.position.letter] = '\0';
        BoardCopy[targetPos.num, targetPos.letter] = PieceToMove.icon;
        PieceToMove.position = targetPos;
    }

    private bool IsKingCheckedSim(KingHelper k)
    {
        string kingPosStr = k.position.PosToString();

        for (int number = 0; number < 8; number++)
        {
            for (int letter = 0; letter < 8; letter++)
            {
                Piece attackingPiece = FindPieceOfPosSim(new BoardPos(number, letter));

                if (attackingPiece != null && attackingPiece.isWhite != k.isWhite)
                {
                    BoardPos kingPos = BoardPos.StringToPos(kingPosStr);

                    if (CheckPieceMovementSim(attackingPiece, kingPos))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private Piece FindPieceOfPosSim(BoardPos position)
    {
        foreach (Piece p in GOCopy)
        {
            if (position.num == p.position.num && position.letter == p.position.letter)
            {
                return p;
            }
        }
        return null;
    }

    private bool CheckPosToMoveSim(Piece p, BoardPos posToMove, bool condition)
    {
        foreach (Piece tempPiece in GOCopy)
        {
            if (posToMove.num == tempPiece.position.num && posToMove.letter == tempPiece.position.letter)
            {
                if (condition)
                {
                    return false;
                }
                else
                {
                    if (p.isWhite != tempPiece.isWhite)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private bool CheckPieceMovementSim(Piece PieceToMove, BoardPos finalPosition)
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
                        if (CheckPosToMoveSim(PieceToMove, finalPosition, true) &&
                                (BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1 ||
                                        (BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 2 &&
                                                PieceToMove.position.num == 1)) &&
                                finalPosition.num - PieceToMove.position.num > 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (CheckPosToMoveSim(PieceToMove, finalPosition, true) &&
                                (BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1 ||
                                        (BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 2 &&
                                                PieceToMove.position.num == 6)) &&
                                PieceToMove.position.num - finalPosition.num > 0)
                        {
                            return true;
                        }
                    }
                }
                else if (movementType.Equals("diagonal") && PieceToMove is PawnHelper)
                {
                    if (PieceToMove.isWhite)
                    {
                        if (!CheckPosToMoveSim(PieceToMove, finalPosition, false) &&
                                BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                                && finalPosition.num - PieceToMove.position.num > 0
                                && FindPieceOfPosSim(finalPosition) != null)
                        {
                            if (FindPieceOfPosSim(finalPosition).isWhite != PieceToMove.isWhite)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (!CheckPosToMoveSim(PieceToMove, finalPosition, false) &&
                                BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                                && PieceToMove.position.num - finalPosition.num > 0
                                && FindPieceOfPosSim(finalPosition) != null)
                        {
                            if (FindPieceOfPosSim(finalPosition).isWhite != PieceToMove.isWhite)
                            {
                                return true;
                            }
                        }
                    }
                }
                else if ((movementType.Equals("vertical") || movementType.Equals("horizontal")) &&
                        PieceToMove is RookHelper)
                {
                    if (!CheckPosToMoveSim(PieceToMove, finalPosition, false) &&
                            !AnyPieceBlockingSim(PieceToMove, finalPosition, movementType))
                    {
                        return true;
                    }
                    else if (CheckPosToMoveSim(PieceToMove, finalPosition, true) &&
                            !AnyPieceBlockingSim(PieceToMove, finalPosition, movementType))
                    {
                        return true;
                    }
                }
                else if ((movementType.Equals("vertical") || movementType.Equals("horizontal")
                        || movementType.Equals("diagonal")) && PieceToMove is QueenHelper)
                {
                    if (!CheckPosToMoveSim(PieceToMove, finalPosition, false) &&
                            !AnyPieceBlockingSim(PieceToMove, finalPosition, movementType))
                    {
                        return true;
                    }
                    else if (CheckPosToMoveSim(PieceToMove, finalPosition, true) &&
                            !AnyPieceBlockingSim(PieceToMove, finalPosition, movementType))
                    {
                        return true;
                    }
                }
                else if ((movementType.Equals("vertical") || movementType.Equals("horizontal") ||
                        movementType.Equals("diagonal")) && PieceToMove is KingHelper)
                {
                    if (!CheckPosToMoveSim(PieceToMove, finalPosition, false) &&
                            !AnyPieceBlockingSim(PieceToMove, finalPosition, movementType) &&
                            BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1)
                    {
                        return true;
                    }
                    else if (CheckPosToMoveSim(PieceToMove, finalPosition, true) &&
                            !AnyPieceBlockingSim(PieceToMove, finalPosition, movementType) &&
                            BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1)
                    {
                        return true;
                    }
                }
                else if (movementType.Equals("knight") && PieceToMove is KnightHelper)
                {
                    Piece targetPiece = FindPieceOfPosSim(finalPosition);
                    if (targetPiece != null)
                    {
                        if (targetPiece.isWhite != PieceToMove.isWhite)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (movementType.Equals("diagonal") && PieceToMove is BishopHelper)
                {
                    if (!CheckPosToMoveSim(PieceToMove, finalPosition, false) &&
                            !AnyPieceBlockingSim(PieceToMove, finalPosition, movementType))
                    {
                        return true;
                    }
                    else if (CheckPosToMoveSim(PieceToMove, finalPosition, true) &&
                            !AnyPieceBlockingSim(PieceToMove, finalPosition, movementType))
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

    private bool AnyPieceBlockingSim(Piece pm, BoardPos finalPos, string movementType)
    {
        Piece pieceToMove = pm;
        int slotsToCheck = BoardPos.SquaresMoved(movementType, pieceToMove.position, finalPos);

        for (int i = 1; i < slotsToCheck; i++)
        {
            try
            {
                if (movementType.Equals("vertical"))
                {
                    if (pieceToMove.position.num < finalPos.num)
                    {
                        if (BoardCopy[pieceToMove.position.num + i, pieceToMove.position.letter] != '\0')
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (BoardCopy[pieceToMove.position.num - i, pieceToMove.position.letter] != '\0')
                        {
                            return true;
                        }
                    }
                }
                else if (movementType.Equals("horizontal"))
                {
                    if (pieceToMove.position.letter < finalPos.letter)
                    {
                        if (BoardCopy[pieceToMove.position.num, pieceToMove.position.letter + i] != '\0')
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (BoardCopy[pieceToMove.position.num, pieceToMove.position.letter - i] != '\0')
                        {
                            return true;
                        }
                    }
                }
                else if (movementType.Equals("diagonal"))
                {
                    if (pieceToMove.position.num < finalPos.num && pieceToMove.position.letter < finalPos.letter)
                    {
                        if (BoardCopy[pieceToMove.position.num + i, pieceToMove.position.letter + i] != '\0')
                        {
                            return true;
                        }
                    }
                    else if (pieceToMove.position.num > finalPos.num && pieceToMove.position.letter < finalPos.letter)
                    {
                        if (BoardCopy[pieceToMove.position.num - i, pieceToMove.position.letter + i] != '\0')
                        {
                            return true;
                        }
                    }
                    else if (pieceToMove.position.num < finalPos.num && pieceToMove.position.letter > finalPos.letter)
                    {
                        if (BoardCopy[pieceToMove.position.num + i, pieceToMove.position.letter - i] != '\0')
                        {
                            return true;
                        }
                    }
                    else if (pieceToMove.position.num > finalPos.num && pieceToMove.position.letter > finalPos.letter)
                    {
                        if (BoardCopy[pieceToMove.position.num - i, pieceToMove.position.letter - i] != '\0')
                        {
                            return true;
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

    private static char[,] DeepCopyBoard(char[,] original)
    {
        char[,] copy = new char[original.GetLength(0), original.GetLength(1)];
        for (int i = 0; i < original.GetLength(0); i++)
        {
            for (int j = 0; j < original.GetLength(1); j++)
            {
                copy[i, j] = original[i, j];
            }
        }
        return copy;
    }

    private static List<Piece> DeepCopyPieces()
    {
        List<Piece> tempBoard = new List<Piece>();
        foreach (Piece piece in PieceManager.AllPieces)
        {
            if (piece is KingHelper)
            {
                tempBoard.Add(new KingHelper((KingHelper)piece));
            }
            else if (piece is QueenHelper)
            {
                tempBoard.Add(new QueenHelper((QueenHelper)piece));
            }
            else if (piece is BishopHelper)
            {
                tempBoard.Add(new BishopHelper((BishopHelper)piece));
            }
            else if (piece is RookHelper)
            {
                tempBoard.Add(new RookHelper((RookHelper)piece));
            }
            else if (piece is PawnHelper)
            {
                tempBoard.Add(new PawnHelper((PawnHelper)piece));
            }
            else if (piece is KnightHelper)
            {
                tempBoard.Add(new KnightHelper((KnightHelper)piece));
            }
        }
        return tempBoard;
    }
}
