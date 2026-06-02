using System;
using System.Collections.Generic;
public abstract class Piece
{
    public bool isWhite;
    public BoardPos position { get; set; }

    //resets every moves + not defiend
    public IObservable<BoardPos> legalMoves; 

    public Piece(bool _isWhite, BoardPos _boardPosition)
    {
        isWhite = _isWhite;
        position = _boardPosition;
    }

    public abstract void Move(BoardPos _boardPosition);

    public abstract bool CheckIfValidMove(BoardPos start, BoardPos end);

    public static Piece FindPieceAtPos(BoardPos Pos)
    {
        throw new System.NotImplementedException();
    }
}
