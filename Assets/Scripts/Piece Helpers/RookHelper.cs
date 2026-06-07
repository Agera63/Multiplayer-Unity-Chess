using System.Collections.Generic;
using UnityEngine;

public class RookHelper : Piece
{
    public bool canCastle;
    private Dictionary<string, string> castlingMap;

    //Constructor for deep copy
    public RookHelper(RookHelper rook) : base(rook.isWhite, rook.position, rook.associatedGameObject) { }
    
    //Constructor for promotion
    public RookHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition, null) { }

    //Default contructor for instantiations
    public RookHelper(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject) : base(_isWhite, _boardPosition, _associatedGameObject)
    {
        icon = _isWhite ? 'R' : 'r';
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
                .StartCoroutine(associatedGameObject.GetComponent<Rook>()
                .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
        }
    }
}
