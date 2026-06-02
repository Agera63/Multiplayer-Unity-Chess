using UnityEngine;

public class KingHelper : Piece
{
    private bool canCastle;

    public KingHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition)
    {
        canCastle = true;
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
