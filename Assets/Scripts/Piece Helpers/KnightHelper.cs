using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class KnightHelper : Piece
{
    public KnightHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition, null) { }
    public KnightHelper(KnightHelper knight) : base(knight.isWhite, knight.position, knight.associatedGameObject) { }
    public KnightHelper(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject) : base(_isWhite, _boardPosition, _associatedGameObject)
    {
        icon = _isWhite ? 'N' : 'n';
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
                .StartCoroutine(associatedGameObject.GetComponent<Knight>()
                .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
        }
    }
}
