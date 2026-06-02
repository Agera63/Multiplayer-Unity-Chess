using System;
using UnityEngine;

public class PawnHelper : Piece
{
    public bool canMove2Squares;
    public event Action promote;

    public PawnHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition)
    {
        //Delcare the legalMove Array
    }

    public override bool CheckIfValidMove(BoardPos start, BoardPos end)
    {
        throw new System.NotImplementedException();
    }

    public override void Move(BoardPos _boardPosition)
    {
        throw new System.NotImplementedException();
    }

    public void Promotion(Piece promotedPawn)
    {

    }

    public IObservable<BoardPos> ReturnLegalMoves()
    {
        throw new System.NotImplementedException();
    }
}
