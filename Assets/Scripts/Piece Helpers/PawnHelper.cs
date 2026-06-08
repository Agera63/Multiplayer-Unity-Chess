using System;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PawnHelper : Piece
{
    public bool canMove2Squares;
    public event Action promote;

    public PawnHelper(PawnHelper pawn) : base(pawn.isWhite, pawn.position, pawn.associatedGameObject) { }
    public PawnHelper(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject) : base(_isWhite, _boardPosition, _associatedGameObject)
    {
        icon = _isWhite ? 'P' : 'p';
        PieceManager.AllPieces.Add(this);
    }

    public override void Move(BoardPos _finalBoardPosition)
    {
        string movementType = BoardPos.CheckMovementDirection(this.position, _finalBoardPosition);
        char[,] temporaryBoard = PieceManager.GetBoard();

        if (!this.CheckPosToMove(this, _finalBoardPosition, true) && movementType.Equals("diagonal"))
        {
            // if we are in this condition, it means that there is a piece of the opposite color that will be removed.
            foreach (Piece p in PieceManager.AllPieces)
            {
                string PStringPosition = p.position.PosToString();
                if (_finalBoardPosition.PosToString().Equals(PStringPosition))
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
            .StartCoroutine(associatedGameObject.GetComponent<Pawn>()
            .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
    }

    public void Promotion(char pieceToPromote)
    {
        PieceType promotionSelection = PieceType.Pawn;
        if (pieceToPromote.ToString().ToLower().Equals("q")) promotionSelection = PieceType.Queen;
        else if (pieceToPromote.ToString().ToLower().Equals("n")) promotionSelection = PieceType.Knight;
        else if (pieceToPromote.ToString().ToLower().Equals("r")) promotionSelection = PieceType.Rook;
        else if (pieceToPromote.ToString().ToLower().Equals("b")) promotionSelection = PieceType.Bishop;

        GameManager.CreateNewPiece(promotionSelection,
            BoardPos.StringToTileVector3(position.PosToString()),
            isWhite);
    }

    public Piece PromotionSimulation(char pieceToPromote)
    {
        switch (pieceToPromote)
        {
            case 'q': return new QueenHelper(this.isWhite, this.position);
            case 'b': return new BishopHelper(this.isWhite, this.position);
            case 'n': return new KnightHelper(this.isWhite, this.position);
            case 'r': return new RookHelper(this.isWhite, this.position);
            default: return null;
        }
    }
}
