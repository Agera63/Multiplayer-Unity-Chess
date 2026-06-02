using UnityEngine;

public class BishopHelper : Piece
{
    public BishopHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition)
    {
        PieceManager.instance.AddPiece(this);
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
