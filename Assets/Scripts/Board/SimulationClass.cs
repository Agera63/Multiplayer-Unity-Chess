using System;
using System.Collections.Generic;

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

        if (simPiece == null) return false;

        char[] movementCharSim = (pieceToMove.position.PosToString() + "-" + finalPos.PosToString()).ToCharArray();
        sc.UpdateSim(movementCharSim);

        // Return true = move is safe (king no longer in check after this move)
        return !sc.IsKingCheckedSim(sc.KingToMove);
    }

    public static bool WillMoveCheckKing(Piece pieceToMove, BoardPos finalPos)
    {
        KingHelper kingToCheck;
        if (pieceToMove is KingHelper)
            kingToCheck = (KingHelper)pieceToMove;
        else
            kingToCheck = pieceToMove.isWhite ? KingHelper.FindWhiteKing() : KingHelper.FindBlackKing();

        if (kingToCheck == null) return false;

        SimulationClass sc = new SimulationClass(kingToCheck);
        Piece simPiece = sc.FindPieceOfPosSim(pieceToMove.position);

        if (simPiece == null) return false;

        char[] movementCharSim = (pieceToMove.position.PosToString() + "-" + finalPos.PosToString()).ToCharArray();
        sc.UpdateSim(movementCharSim);

        // true = king is safe after this move
        return !sc.IsKingCheckedSim(sc.KingToMove);
    }

    public static bool IsCheckMate(KingHelper k)
    {
        if (!k.IsChecked())
            return false;

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
                                return false;
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
            if (p.position.letter == K.position.letter && p.position.num == K.position.num && p is KingHelper)
                return (KingHelper)p;
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
                                    GOCopy.Add(new KnightHelper((KnightHelper)((PawnHelper)p).PromotionSimulation(MovementChar[5])));
                                    temp = false; break;
                                case 'q':
                                    GOCopy.Add(new QueenHelper((QueenHelper)((PawnHelper)p).PromotionSimulation(MovementChar[5])));
                                    temp = false; break;
                                case 'r':
                                    GOCopy.Add(new RookHelper((RookHelper)((PawnHelper)p).PromotionSimulation(MovementChar[5])));
                                    temp = false; break;
                                case 'b':
                                    GOCopy.Add(new BishopHelper((BishopHelper)((PawnHelper)p).PromotionSimulation(MovementChar[5])));
                                    temp = false; break;
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
                GOCopy.RemoveAt(i);
        }
    }

    private void MovementSim(BoardPos targetPos, Piece PieceToMove)
    {
        Piece targetPiece = FindPieceOfPosSim(targetPos);
        if (targetPiece != null && targetPiece.isWhite != PieceToMove.isWhite)
            targetPiece.isActive = false;

        // Handle en passant capture in simulation
        if (PieceToMove is PawnHelper)
        {
            int direction = PieceToMove.isWhite ? -1 : 1;
            bool isDiagonal = PieceToMove.position.letter != targetPos.letter;
            bool isEmptySquare = targetPiece == null;

            if (isDiagonal && isEmptySquare)
            {
                // The captured pawn sits beside the moving pawn, not on the destination
                Piece enPassantPawn = FindPieceOfPosSim(new BoardPos(targetPos.num + direction, targetPos.letter));
                if (enPassantPawn != null && enPassantPawn is PawnHelper && enPassantPawn.isWhite != PieceToMove.isWhite)
                {
                    enPassantPawn.isActive = false;
                    BoardCopy[enPassantPawn.position.num, enPassantPawn.position.letter] = '\0';
                }
            }
        }

        BoardCopy[PieceToMove.position.num, PieceToMove.position.letter] = '\0';
        BoardCopy[targetPos.num, targetPos.letter] = PieceToMove.icon;
        PieceToMove.position = targetPos;
    }

    private bool IsKingCheckedSim(KingHelper k)
    {
        BoardPos kingPos = k.position;

        for (int number = 0; number < 8; number++)
        {
            for (int letter = 0; letter < 8; letter++)
            {
                Piece attackingPiece = FindPieceOfPosSim(new BoardPos(number, letter));

                if (attackingPiece != null && attackingPiece.isWhite != k.isWhite)
                {
                    if (CheckPieceMovementSim(attackingPiece, kingPos))
                        return true;
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
                return p;
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
                    return false;
                else if (p.isWhite != tempPiece.isWhite)
                    return false;
            }
        }
        return true;
    }

    private bool CheckPieceMovementSim(Piece PieceToMove, BoardPos finalPosition)
    {
        try
        {
            string movementType = BoardPos.CheckMovementDirection(PieceToMove.position, finalPosition);
            if (movementType == null) return false;

            if (movementType.Equals("vertical") && PieceToMove is PawnHelper)
            {
                // Pawns moving forward do NOT threaten the king — only diagonals do.
                // FIX 5: Old sim treated vertical pawn moves as threats. Removed entirely.
                return false;
            }
            else if (movementType.Equals("diagonal") && PieceToMove is PawnHelper)
            {
                if (PieceToMove.isWhite)
                {
                    if (BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                        && finalPosition.num - PieceToMove.position.num > 0)
                        return true;
                }
                else
                {
                    if (BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1
                        && PieceToMove.position.num - finalPosition.num > 0)
                        return true;
                }
            }
            else if ((movementType.Equals("vertical") || movementType.Equals("horizontal")) && PieceToMove is RookHelper)
            {
                if (!AnyPieceBlockingSim(PieceToMove, finalPosition, movementType))
                    return true;
            }
            else if ((movementType.Equals("vertical") || movementType.Equals("horizontal") || movementType.Equals("diagonal")) && PieceToMove is QueenHelper)
            {
                if (!AnyPieceBlockingSim(PieceToMove, finalPosition, movementType))
                    return true;
            }
            else if ((movementType.Equals("vertical") || movementType.Equals("horizontal") || movementType.Equals("diagonal")) && PieceToMove is KingHelper)
            {
                if (BoardPos.SquaresMoved(movementType, PieceToMove.position, finalPosition) == 1)
                    return true;
            }
            else if (movementType.Equals("knight") && PieceToMove is KnightHelper)
            {
                return true;
            }
            else if (movementType.Equals("diagonal") && PieceToMove is BishopHelper)
            {
                if (!AnyPieceBlockingSim(PieceToMove, finalPosition, movementType))
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool AnyPieceBlockingSim(Piece pm, BoardPos finalPos, string movementType)
    {
        int slotsToCheck = BoardPos.SquaresMoved(movementType, pm.position, finalPos);

        for (int i = 1; i < slotsToCheck; i++)
        {
            try
            {
                if (movementType.Equals("vertical"))
                {
                    int row = pm.position.num < finalPos.num ? pm.position.num + i : pm.position.num - i;
                    if (BoardCopy[row, pm.position.letter] != '\0') return true;
                }
                else if (movementType.Equals("horizontal"))
                {
                    int col = pm.position.letter < finalPos.letter ? pm.position.letter + i : pm.position.letter - i;
                    if (BoardCopy[pm.position.num, col] != '\0') return true;
                }
                else if (movementType.Equals("diagonal"))
                {
                    int row = pm.position.num < finalPos.num ? pm.position.num + i : pm.position.num - i;
                    int col = pm.position.letter < finalPos.letter ? pm.position.letter + i : pm.position.letter - i;
                    if (BoardCopy[row, col] != '\0') return true;
                }
            }
            catch
            {
                return true;
            }
        }
        return false;
    }

    private static char[,] DeepCopyBoard(char[,] original)
    {
        char[,] copy = new char[original.GetLength(0), original.GetLength(1)];
        Array.Copy(original, copy, original.Length);
        return copy;
    }

    private static List<Piece> DeepCopyPieces()
    {
        List<Piece> tempBoard = new List<Piece>();
        foreach (Piece piece in PieceManager.AllPieces)
        {
            if (!piece.isActive) continue;

            if (piece is KingHelper) tempBoard.Add(new KingHelper((KingHelper)piece));
            else if (piece is QueenHelper) tempBoard.Add(new QueenHelper((QueenHelper)piece));
            else if (piece is BishopHelper) tempBoard.Add(new BishopHelper((BishopHelper)piece));
            else if (piece is RookHelper) tempBoard.Add(new RookHelper((RookHelper)piece));
            else if (piece is PawnHelper) tempBoard.Add(new PawnHelper((PawnHelper)piece));
            else if (piece is KnightHelper) tempBoard.Add(new KnightHelper((KnightHelper)piece));
        }
        return tempBoard;
    }
}