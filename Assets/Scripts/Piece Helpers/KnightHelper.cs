using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class KnightHelper : Piece
{
    public KnightHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition, null) { icon = _isWhite ? 'N' : 'n'; }
    public KnightHelper(KnightHelper knight) : base(knight.isWhite, knight.position, knight.associatedGameObject) { icon = knight.isWhite ? 'N' : 'n'; }
    public KnightHelper(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject) : base(_isWhite, _boardPosition, _associatedGameObject)
    {
        icon = _isWhite ? 'N' : 'n';
        PieceManager.AllPieces.Add(this);
    }

    public override void Move(BoardPos _finalBoardPosition)
    {
        char[,] temporaryBoard = PieceManager.GetBoard();

        if (!this.CheckPosToMove(this, _finalBoardPosition, true))
        {
            // if we are in this condition, it means that there is a piece of the opposite color that will be removed.
            foreach (Piece p in PieceManager.AllPieces)
            {
                string PStringPosition = p.position.PosToString();
                if (_finalBoardPosition.PosToString().Equals(PStringPosition) && p.isActive)
                {
                    p.isActive = false;
                    temporaryBoard[this.position.num, this.position.letter] = '\0';
                    temporaryBoard[_finalBoardPosition.num, _finalBoardPosition.letter] = this.icon;
                    this.position = _finalBoardPosition;
                    break;
                }
            }
        }
        else
        {
            temporaryBoard[this.position.num, this.position.letter] = '\0';
            temporaryBoard[_finalBoardPosition.num, _finalBoardPosition.letter] = this.icon;
            this.position = _finalBoardPosition;
        }
        PieceManager.SetBoard(temporaryBoard);
        associatedGameObject.GetComponent<MonoBehaviour>()
            .StartCoroutine(associatedGameObject.GetComponent<Knight>()
            .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
    }
}