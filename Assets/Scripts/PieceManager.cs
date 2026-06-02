using System.Collections.Generic;

public class PieceManager
{
    public static PieceManager instance = new(); //Singleton

    public static char[,] Board = new char[8, 8];
    private HashSet<Piece> AllPieces = new();

    public void UpdateBoardState()
    {

    }

    public static bool CanCastle(RookHelper rook, KingHelper king)
    {
        return rook.canCastle && king.canCastle && king.isWhite == rook.isWhite;
    }

    public void AddPiece(Piece pieceToAdd) => AllPieces.Add(pieceToAdd);
    public void RemovePiece(Piece pieceToRemove) => AllPieces.Add(pieceToRemove);
}
