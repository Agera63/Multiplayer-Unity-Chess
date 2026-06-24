using System;
using System.Linq;
using UnityEngine;

public class PawnHelper : Piece
{
    public bool canMove2Squares;
    public bool canBeEnPassant;
    public event Action<PieceType, Vector3, Piece> promote;

    public PawnHelper(PawnHelper pawn) : base(pawn.isWhite, pawn.position, pawn.associatedGameObject) { icon = pawn.isWhite ? 'P' : 'p'; }
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
            // Check if this is an en passant capture
            if (movementType.Equals("diagonal"))
            {
                int capturedPawnRow = this.isWhite ? _finalBoardPosition.num - 1 : _finalBoardPosition.num + 1;
                Piece capturedPawn = FindPieceAtPos(new BoardPos(capturedPawnRow, _finalBoardPosition.letter));
                if (capturedPawn != null && capturedPawn is PawnHelper && capturedPawn.isWhite != this.isWhite)
                {
                    capturedPawn.isActive = false;
                    temporaryBoard[capturedPawn.position.num, capturedPawn.position.letter] = '\0';
                }
            }

            temporaryBoard[this.position.num, this.position.letter] = '\0';
            temporaryBoard[_finalBoardPosition.num, _finalBoardPosition.letter] = this.icon;
            this.position = _finalBoardPosition;
        }

        PieceManager.SetBoard(temporaryBoard);

        associatedGameObject.GetComponent<MonoBehaviour>()
            .StartCoroutine(associatedGameObject.GetComponent<Pawn>()
            .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
    }

    /// <summary>
    /// Handles the promotion of a pawn by converting the given promotion character
    /// into a <see cref="PieceType"/> and firing the <see cref="promote"/> event
    /// to trigger the creation of the new piece in the game world.
    /// </summary>
    /// <param name="pieceToPromote">
    /// A char representing the piece to promote to 
    /// ('q' for Queen, 'r' for Rook, 'b' for Bishop, 'n' for Knight).
    /// </param>
    public void Promotion(char pieceToPromote)
    {
        PieceType promotionSelection = PieceType.Pawn;
        if (pieceToPromote.ToString().ToLower().Equals("q")) promotionSelection = PieceType.Queen;
        else if (pieceToPromote.ToString().ToLower().Equals("n")) promotionSelection = PieceType.Knight;
        else if (pieceToPromote.ToString().ToLower().Equals("r")) promotionSelection = PieceType.Rook;
        else if (pieceToPromote.ToString().ToLower().Equals("b")) promotionSelection = PieceType.Bishop;

        promote?.Invoke(promotionSelection, BoardPos.StringToTileVector3(position.PosToString()), this);
    }

    /// <summary>
    /// Creates and returns a new simulation-only piece of the promoted type,
    /// used by <see cref="SimulationClass"/> to validate moves involving pawn promotion
    /// without affecting the actual game state.
    /// </summary>
    /// <param name="pieceToPromote">
    /// A char representing the piece to promote to
    /// ('q' for Queen, 'r' for Rook, 'b' for Bishop, 'n' for Knight).
    /// </param>
    /// <returns>A new <see cref="Piece"/> of the promoted type, or null if the char is unrecognized.</returns>
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

    /// <summary>
    /// Checks whether a given callback is already subscribed to the <see cref="promote"/> event,
    /// preventing duplicate subscriptions.
    /// </summary>
    /// <param name="creationMethod">The callback to check for in the promote event's invocation list.</param>
    /// <returns>True if the callback is already subscribed, false otherwise.</returns>
    public bool CheckPromotionActions(Action<PieceType, Vector3, Piece> creationMethod) { return promote?.GetInvocationList().Contains(creationMethod) ?? false; }
}