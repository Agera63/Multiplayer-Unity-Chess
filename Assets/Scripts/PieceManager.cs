using System;
using System.Collections.Generic;
using System.Linq;

public class PieceManager
{
    private static char[,] Board = new char[8, 8];
    public static HashSet<Piece> AllPieces = new();

    public static event Action<Piece> removeGameObj;

    /// <summary>
    /// Getter for the board with the pieces
    /// </summary>
    /// <returns>the chess board</returns>
    public static char[,] GetBoard() { return Board; }

    /// <summary>
    /// Setter for the board
    /// </summary>
    /// <param name="board">the new board to be set</param>
    public static void SetBoard(char[,] board) { Board = board; }

    /// <summary>
    /// This is called to update all the pieces which has been moved.
    /// </summary>
    /// <param name="MovementChar">has the beginning and the destination</param>
    public static void Update(char[] MovementChar)
    {
        string PieceToMove = MovementChar[0].ToString() + MovementChar[1].ToString().ToLower();
        string PositionToMove = MovementChar[3].ToString() + MovementChar[4].ToString().ToLower();

        foreach (Piece p in AllPieces.ToList())
        {
            if (p.position.PosToString().Equals(PieceToMove.ToLower()))
            {
                p.Move(BoardPos.StringToPos(PositionToMove.ToLower()));
                if (MovementChar.Length == 6 && MovementChar[5] != '\0' && p is PawnHelper)
                {
                    if ((p.position.num == 0 && !GameModeManager.instance.playerColor) || (p.position.num == 7 && GameModeManager.instance.playerColor))
                    {
                        ((PawnHelper)p).Promotion(MovementChar[5]);
                        p.isActive = false;
                    }
                }
                break;
            }
        }
        // checks for deactivated pieces
        foreach (Piece p in AllPieces)
        {
            if (!p.isActive)
            {
                removeGameObj?.Invoke(p);
                AllPieces.Remove(p);
                /*DO NOT REMOVE BREAK!!!
                If you remove the break, the list would have been altered and then it will throw an error */
                break;
            }
        }

        InitializeBoard();
    }

    /// <summary>
    /// Checking to see if we can castle the rook and the king
    /// </summary>
    /// <param name="r">The rook we are checking</param>
    /// <param name="k">The King we are checking</param>
    /// <returns>true if can castle / false if cannot</returns>
    public static bool CanCastle(RookHelper r, KingHelper k)
    {
        return k.canCastle && r.canCastle && k.isWhite == r.isWhite;
    }

    public static void InitializeBoard()
    {
        Board = new char[8, 8];
        foreach (Piece p in AllPieces)
        {
            if (p.isActive)
            {
                Board[p.position.num, p.position.letter] = p.icon;
            }
        }
    }
}