using System;
using System.Collections.Generic;
using UnityEngine;

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

    public override bool IsValidMove(BoardPos start, BoardPos end)
    {
        throw new System.NotImplementedException();
    }

    public override void Move(BoardPos _finalBoardPosition)
    {
        if(IsValidMove(position, _finalBoardPosition))
        {
            associatedGameObject.GetComponent<MonoBehaviour>()
                .StartCoroutine(associatedGameObject.GetComponent<Pawn>()
                .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
        }
    }

    public void Promotion(char pieceToPromote)
    {
        PieceType promotionSelection = PieceType.Pawn;
        if (pieceToPromote.ToString().ToLower().Equals("q")) promotionSelection = PieceType.Queen;
        else if(pieceToPromote.ToString().ToLower().Equals("n")) promotionSelection = PieceType.Knight;
        else if(pieceToPromote.ToString().ToLower().Equals("r")) promotionSelection = PieceType.Rook;
        else if(pieceToPromote.ToString().ToLower().Equals("b")) promotionSelection = PieceType.Bishop;

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
