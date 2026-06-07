using System.Collections.Generic;
using UnityEngine;

public class QueenHelper : Piece
{
    public QueenHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition, null) { }
    public QueenHelper(QueenHelper queen) : base(queen.isWhite, queen.position, queen.associatedGameObject) { }
    public QueenHelper(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject) : base(_isWhite, _boardPosition, _associatedGameObject)
    {
        icon = _isWhite ? 'Q' : 'q';
        
        PieceManager.AllPieces.Add(this);
    }

    public override bool IsValidMove(BoardPos start, BoardPos end)
    {
        throw new System.NotImplementedException();
    }

    public override void Move(BoardPos _finalBoardPosition)
    {
        if (IsValidMove(position, _finalBoardPosition))
        {
            associatedGameObject.GetComponent<MonoBehaviour>()
                .StartCoroutine(associatedGameObject.GetComponent<Queen>()
                .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
        }
    }
}
