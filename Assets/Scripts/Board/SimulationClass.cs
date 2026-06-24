using System;
using System.Collections.Generic;

public class SimulationClass
{
    private char[,] BoardCopy;
    private List<Piece> GOCopy;
    private KingHelper KingToMove;

    /// <summary>
    /// Initializes a new simulation by deep copying the current board state and piece list,
    /// then locating the king to track within the simulation.
    /// </summary>
    /// <param name="k">The king to track during the simulation.</param>
    private SimulationClass(KingHelper k)
    {
        BoardCopy = DeepCopyBoard(PieceManager.GetBoard());
        GOCopy = DeepCopyPieces();
        KingToMove = FindKing(k);
    }

    /// <summary>
    /// Simulates a move and checks whether it resolves the king being in check.
    /// Used when the king is already in check to validate that a move gets it out.
    /// </summary>
    /// <param name="pieceToMove">The piece attempting to move.</param>
    /// <param name="finalPos">The target position of the move.</param>
    /// <param name="k">The king currently in check.</param>
    /// <returns>True if the move successfully removes the king from check, false otherwise.</returns>
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

    /// <summary>
    /// Simulates a move and checks whether it would leave the friendly king in check.
    /// Used when the king is not currently in check to ensure the move doesn't create one.
    /// </summary>
    /// <param name="pieceToMove">The piece attempting to move.</param>
    /// <param name="finalPos">The target position of the move.</param>
    /// <returns>True if the move is safe and does not put the friendly king in check, false otherwise.</returns>
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

    /// <summary>
    /// Checks whether the given king is in checkmate by verifying it is in check
    /// and that no legal move exists for any friendly piece to resolve it.
    /// </summary>
    /// <param name="k">The king to check for checkmate.</param>
    /// <returns>True if the king is in checkmate, false otherwise.</returns>
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

    /// <summary>
    /// Finds the simulated copy of the given king within the deep-copied piece list.
    /// </summary>
    /// <param name="K">The real king to find the simulation equivalent of.</param>
    /// <returns>The simulated <see cref="KingHelper"/> copy, or null if not found.</returns>
    private KingHelper FindKing(KingHelper K)
    {
        foreach (Piece p in GOCopy)
        {
            if (p.position.letter == K.position.letter && p.position.num == K.position.num && p is KingHelper)
                return (KingHelper)p;
        }
        return null;
    }

    /// <summary>
    /// Applies a move to the simulation's deep-copied board and piece list,
    /// handling captures, en passant, and pawn promotion within the simulation.
    /// </summary>
    /// <param name="MovementChar">A char array encoding the move in the format "e2-e4",
    /// with an optional 6th char for pawn promotion (e.g. 'q').</param>
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

    /// <summary>
    /// Moves a piece within the simulation, handling captures and en passant
    /// without affecting the actual game state.
    /// </summary>
    /// <param name="targetPos">The target position to move the piece to.</param>
    /// <param name="PieceToMove">The simulated piece to move.</param>
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

    /// <summary>
    /// Checks whether the given simulated king is in check by testing
    /// whether any enemy piece can legally attack its position.
    /// </summary>
    /// <param name="k">The simulated king to check.</param>
    /// <returns>True if the king is in check, false otherwise.</returns>
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

    /// <summary>
    /// Finds a simulated piece at the given board position within the deep-copied piece list.
    /// </summary>
    /// <param name="position">The board position to search at.</param>
    /// <returns>The simulated <see cref="Piece"/> at that position, or null if none exists.</returns>
    private Piece FindPieceOfPosSim(BoardPos position)
    {
        foreach (Piece p in GOCopy)
        {
            if (position.num == p.position.num && position.letter == p.position.letter)
                return p;
        }
        return null;
    }

    /// <summary>
    /// Checks whether a simulated piece can move to the target position based on occupancy,
    /// mirroring <see cref="Piece.CheckPosToMove"/> but operating on the deep-copied piece list.
    /// </summary>
    /// <param name="p">The simulated piece attempting to move.</param>
    /// <param name="posToMove">The target position to check.</param>
    /// <param name="condition">
    /// If true, checks for any piece at the target position (blocked by both friendly and enemy).
    /// If false, checks only for a friendly piece at the target position (can still capture enemies).
    /// </param>
    /// <returns>True if the move to the target position is allowed, false otherwise.</returns>
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

    /// <summary>
    /// Checks whether a simulated piece's movement to the target position follows the rules
    /// for its piece type, mirroring <see cref="Piece.CheckPieceMovement"/> but operating
    /// on the deep-copied board and piece list.
    /// </summary>
    /// <param name="PieceToMove">The simulated piece attempting to move.</param>
    /// <param name="finalPosition">The target position of the move.</param>
    /// <returns>True if the movement is valid for the given piece type, false otherwise.</returns>
    private bool CheckPieceMovementSim(Piece PieceToMove, BoardPos finalPosition)
    {
        try
        {
            string movementType = BoardPos.CheckMovementDirection(PieceToMove.position, finalPosition);
            if (movementType == null) return false;

            if (movementType.Equals("vertical") && PieceToMove is PawnHelper)
            {
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

    /// <summary>
    /// Checks whether any piece is blocking the path between a simulated piece and its target position,
    /// mirroring <see cref="Piece.AnyPieceBlocking"/> but operating on the deep-copied board.
    /// </summary>
    /// <param name="pm">The simulated piece attempting to move.</param>
    /// <param name="finalPos">The target position to check the path towards.</param>
    /// <param name="movementType">The type of movement being performed (vertical, horizontal, or diagonal).</param>
    /// <returns>True if any piece is blocking the path, false otherwise.</returns>
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

    /// <summary>
    /// Creates a deep copy of the given 8x8 char board array.
    /// </summary>
    /// <param name="original">The board to copy.</param>
    /// <returns>A new 8x8 char array with the same values as the original.</returns>
    private static char[,] DeepCopyBoard(char[,] original)
    {
        char[,] copy = new char[original.GetLength(0), original.GetLength(1)];
        Array.Copy(original, copy, original.Length);
        return copy;
    }

    /// <summary>
    /// Creates a deep copy of all active pieces in <see cref="PieceManager.AllPieces"/>,
    /// using each piece type's copy constructor to preserve state without affecting the originals.
    /// </summary>
    /// <returns>A list of deep-copied <see cref="Piece"/> objects representing the current game state.</returns>
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