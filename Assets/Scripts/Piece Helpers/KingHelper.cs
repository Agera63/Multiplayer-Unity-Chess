using UnityEngine;

public class KingHelper : Piece
{
    public bool canCastle;

    public KingHelper(KingHelper king) : base(king.isWhite, king.position, king.associatedGameObject) { icon = king.isWhite ? 'K' : 'k'; canCastle = king.canCastle; }
    public KingHelper(bool _isWhite, BoardPos _boardPosition, GameObject _associatedGameObject) : base(_isWhite, _boardPosition, _associatedGameObject)
    {
        icon = _isWhite ? 'K' : 'k';
        canCastle = true;

        PieceManager.AllPieces.Add(this);
    }

    public override void Move(BoardPos _finalBoardPosition)
    {
        char[,] temporaryBoard = PieceManager.GetBoard();
        string strPosition = _finalBoardPosition.PosToString();

        // Checks if we want to castle the king
        if ((strPosition.Equals("g1") || strPosition.Equals("c1") ||
                strPosition.Equals("g8") || strPosition.Equals("c8")) && !this.IsChecked())
        {
            RookHelper r = (RookHelper)Piece.FindPieceAtPos(BoardPos.StringToPos(Piece.castlingMap[strPosition]));
            if (PieceManager.CanCastle(r, this))
            {
                temporaryBoard[this.position.num, this.position.letter] = '\0';
                temporaryBoard[BoardPos.StringToPos(strPosition).num, BoardPos.StringToPos(strPosition).letter] = this.icon;
                this.position = BoardPos.StringToPos(strPosition);
                temporaryBoard = r.CastleMovement(temporaryBoard, strPosition);
            }
        }
        else
        {
            if (!this.CheckPosToMove(this, BoardPos.StringToPos(strPosition), true))
            {
                // if we are in this condition, it means that there is a piece of the opposite color that will be removed.
                foreach (Piece p in PieceManager.AllPieces)
                {
                    string PStringPosition = p.position.PosToString();
                    if (strPosition.Equals(PStringPosition) && p.isActive)
                    {
                        p.isActive = false;
                        temporaryBoard[this.position.num, this.position.letter] = '\0';
                        temporaryBoard[BoardPos.StringToPos(strPosition).num, BoardPos.StringToPos(strPosition).letter] = this.icon;
                        this.position = BoardPos.StringToPos(strPosition);
                        break;
                    }
                }
            }
            else
            {
                temporaryBoard[this.position.num, this.position.letter] = '\0';
                temporaryBoard[BoardPos.StringToPos(strPosition).num, BoardPos.StringToPos(strPosition).letter] = this.icon;
                this.position = BoardPos.StringToPos(strPosition);
            }
            canCastle = false;
        }
        canCastle = false;
        PieceManager.SetBoard(temporaryBoard);
        associatedGameObject.GetComponent<MonoBehaviour>()
            .StartCoroutine(associatedGameObject.GetComponent<King>()
            .MoveAnimation(BoardPos.StringToTileVector3(_finalBoardPosition.PosToString())));
    }

    /// <summary>
    /// Checks if the king is currently in check
    /// </summary>
    /// <returns>true if in check / false if not in check</returns>
    public bool IsChecked()
    {
        // Get King position (DONE)
        string kingPosStr = this.position.PosToString();

        // First for is number position, second for is letter position
        for (int number = 0; number < 8; number++)
        {
            for (int letter = 0; letter < 8; letter++)
            {
                if (FindPieceAtPos(new BoardPos(number, letter)) != null)
                {
                    char[] movementChart = ((new BoardPos(number, letter).PosToString()) + "-" + kingPosStr).ToCharArray();

                    BoardPos finalPosition = BoardPos.StringToPos(movementChart[3].ToString() +
                            movementChart[4].ToString().ToLower());
                    Piece PieceToMove = FindPieceAtPos(BoardPos.StringToPos(movementChart[0].ToString() +
                            movementChart[1].ToString().ToLower()));

                    // Check if valid move
                    if (Piece.CheckPieceMovement(PieceToMove, finalPosition) && this.isWhite != PieceToMove.isWhite)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Finds the white king
    /// </summary>
    /// <returns>the white king</returns>
    public static KingHelper FindWhiteKing()
    {
        foreach (Piece p in PieceManager.AllPieces)
        {
            if (p is KingHelper && p.isWhite)
            {
                return (KingHelper)p;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the black king
    /// </summary>
    /// <returns>the black king</returns>
    public static KingHelper FindBlackKing()
    {
        foreach (Piece p in PieceManager.AllPieces)
        {
            if (p is KingHelper && !p.isWhite)
            {
                return (KingHelper)p;
            }
        }
        return null;
    }
}