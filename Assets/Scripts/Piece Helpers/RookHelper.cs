using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class RookHelper : Piece
{
    public bool canCastle;
    private Dictionary<string, string> castlePositions;

    //Constructor for deep copy
    public RookHelper(RookHelper rook) : base(rook.isWhite, rook.position, rook.associatedGameObject) { icon = rook.isWhite ? 'R' : 'r'; canCastle = true; }

    //Constructor for promotion
    public RookHelper(bool _isWhite, BoardPos _boardPosition) : base(_isWhite, _boardPosition, null) { icon = _isWhite ? 'R' : 'r'; canCastle = true; }

    //Default contructor for instantiations
    public RookHelper(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject) : base(_isWhite, _boardPosition, _associatedGameObject)
    {
        icon = _isWhite ? 'R' : 'r';
        canCastle = true;
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
        canCastle = false;
        associatedGameObject.GetComponent<MonoBehaviour>()
            .StartCoroutine(associatedGameObject.GetComponent<Rook>()
            .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
    }

    public char[,] CastleMovement(char[,] temporaryBoard, string placeToMove)
    {
        if (castlePositions == null)
        {
            InitializeCastlePositions();
        }

        // We change the place to move from the kings place to the rooks place based on the king movement
        placeToMove = castlePositions[Piece.castlingMap[placeToMove]];
        temporaryBoard[this.position.num, this.position.letter] = '\0';
        temporaryBoard[BoardPos.StringToPos(placeToMove).num, BoardPos.StringToPos(placeToMove).letter] = this.icon;
        this.position = BoardPos.StringToPos(placeToMove);

        canCastle = false;
        associatedGameObject.GetComponent<MonoBehaviour>()
            .StartCoroutine(associatedGameObject.GetComponent<Rook>()
            .MoveAnimation(BoardPos.StringToTileVector3(placeToMove)));

        return temporaryBoard;
    }

    private void InitializeCastlePositions()
    {
        castlePositions = new Dictionary<string, string>();
        castlePositions.Add(Piece.castlingMap["g1"], "f1");
        castlePositions.Add(Piece.castlingMap["c1"], "d1");
        castlePositions.Add(Piece.castlingMap["g8"], "f8");
        castlePositions.Add(Piece.castlingMap["c8"], "d8");
    }
}