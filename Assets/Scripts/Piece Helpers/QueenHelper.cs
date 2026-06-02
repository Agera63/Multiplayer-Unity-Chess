using UnityEngine;

public class QueenHelper : Piece
{
    public QueenHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition)
    {
    }

    public override bool CheckIfValidMove(BoardPos start, BoardPos end)
    {
        throw new System.NotImplementedException();
    }

    public override void Move(BoardPos _boardPosition)
    {
        throw new System.NotImplementedException();
    }
}
