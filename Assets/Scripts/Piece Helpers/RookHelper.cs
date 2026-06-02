using System.Collections.Generic;
using UnityEngine;

public class RookHelper : Piece
{
    public bool canCastle;
    private Dictionary<string, string> castlingMap;
    public RookHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition)
    {
        InitializeCastlePositions();
    }

    public override bool CheckIfValidMove(BoardPos start, BoardPos end)
    {
        throw new System.NotImplementedException();
    }

    public override void Move(BoardPos _boardPosition)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Creates the castling map for the rook
     */
    private void InitializeCastlePositions()
    {
        castlingMap = new Dictionary<string, string>();
        castlingMap.Add("g1", "h1");
        castlingMap.Add("c1", "a1");
        castlingMap.Add("g8", "h8");
        castlingMap.Add("c8", "a8");
    }
}
