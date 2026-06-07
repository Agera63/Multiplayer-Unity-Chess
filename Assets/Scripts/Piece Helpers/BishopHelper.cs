using System.Collections.Generic;
using UnityEngine;

public class BishopHelper : Piece
{
    public BishopHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition, null) { }
    public BishopHelper(BishopHelper bishop) : base(bishop.isWhite, bishop.position, bishop.associatedGameObject) { }
    public BishopHelper(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject) : base(_isWhite, _boardPosition , _associatedGameObject)
    {
        icon = _isWhite ? 'B' : 'b';
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
                .StartCoroutine(associatedGameObject.GetComponent<Bishop>()
                .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
        }
    }
}
