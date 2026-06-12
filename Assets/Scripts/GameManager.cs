using System.Collections;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject[] allPieceModels;
    [SerializeField] GameObject player;
    [SerializeField] GameObject gameOverText;
        
    private bool colorTurn;
    private GameObject selectedPiece;
    private Piece pieceHelperScript;
    private char[] mouvementChar;
    private bool promotionSelectionMade;

    private IPlayerController whiteController;
    private IPlayerController blackController;
    private IPlayerController currentController;

    private bool whoWon;

    void Awake()
    {
        //Initalizes the player position
        if (GameModeManager.instance.playerColor) Instantiate(player, new Vector3(0.5f, 5.5f, -2.5f), Quaternion.Euler(new Vector3(45, 0,0)));
        else Instantiate(player, new Vector3(0.5f, 5.5f, 10f), Quaternion.Euler(new Vector3(45, 180, 0)));

        colorTurn = true;
        selectedPiece = null;

        PieceManager.InitializeBoard();
        PieceManager.removeGameObj += DestroyPiece;

        AssignControllers();

        StartTurn();
    }

    void Update()
    {
        //BUG: when a promtoion is being made, another click is registered and the finalposition of the pawn move is wrong
        //FIX: when the game is paused, it only comes from promotion menu being active. So, no clicks are registered
        if (Time.timeScale == 0f) return;

        if (currentController != null && currentController.IsHuman)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                GameObject clickedObject = ClickHelper.FindClickedObject();

                if (isValidPiece(clickedObject))
                    return;

                if (selectedPiece is not null && pieceHelperScript.legalMoves.Count != 0)
                {
                    if (clickedObject.TryGetComponent(out BoardTile bt))
                    {
                        string moveString = pieceHelperScript.position.PosToString() + "-" + bt.boardPosition;
                        humanController?.MakeMove(moveString);
                    }
                    else if (clickedObject.TryGetComponent(out MonoBehaviorPiece MBp)
                        && MBp.isWhite != GameModeManager.instance.playerColor)
                    {
                        string moveString = pieceHelperScript.position.PosToString() + "-" + MBp.helperClass.position.PosToString();
                        humanController?.MakeMove(moveString);
                    }
                }
            }
        }
    }

    private void StartTurn()
    {
        currentController = colorTurn ? whiteController : blackController;
        currentController.StartTurn((moveString) => TryMovePiece(moveString));
    }

    private void TryMovePiece(string moveString)
    {
        mouvementChar = new char[5];
        mouvementChar = moveString.ToCharArray();

        //Checks if moving piece is a pawn to add the type of piece it wants to promote in the mouvement char
        Piece movingPiece = Piece.FindPieceAtPos(BoardPos.StringToPos(moveString.Split('-')[0]));
        
        //Handles the pawn promotion move
        if (movingPiece is PawnHelper 
            && movingPiece.associatedGameObject.TryGetComponent(out Pawn p) 
            && p.isGoignToPromoting)
        {
            StartCoroutine(HandlePromotion(mouvementChar, p)); 
            return;
        }

        //Handles other mouvements that are not promotions
        ExecuteMove();
    }

    private void ExecuteMove()
    {
        //Sees if the move is valid before executing it
        if (Piece.IsValidMove(mouvementChar, colorTurn))
        {
            //Updates the board 
            PieceManager.Update(mouvementChar);
            colorTurn = !colorTurn;

            selectedPiece = null;
            pieceHelperScript = null;

            //Checks if game is over
            if (IsGameOver())
                StartCoroutine(CheckMateSteps());

            //Starts the next turn after every successful move
            StartTurn();
        }
    }

    private void AssignControllers()
    {
        bool playerIsWhite = GameModeManager.instance.playerColor;

        switch (GameModeManager.instance.selectedMode)
        {
            case GameMode.PvE:
                if (playerIsWhite)
                {
                    whiteController = new HumanController();
                    blackController = new StockfishController(this, false);
                }
                else
                {
                    whiteController = new StockfishController(this, true);
                    blackController = new HumanController();
                }
                break;

            case GameMode.PvP_Online:
                if (playerIsWhite)
                {
                    whiteController = new HumanController();
                    blackController = new NetworkController();
                }
                else
                {
                    whiteController = new NetworkController();
                    blackController = new HumanController();
                }
                break;
        }
    }

    private HumanController humanController => currentController as HumanController;

    private bool isValidPiece(GameObject clickedObject)
    {
        if (pieceHelperScript is not null) pieceHelperScript.HideLegalMoves();

        if (clickedObject.TryGetComponent(out Pawn pawn)
            && pawn.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = pawn.helperClass;
            pawn.ShowLegalMoves();
            if (!((PawnHelper)pawn.helperClass).CheckPromotionActions(PawnPromotion))
                ((PawnHelper)pawn.helperClass).promote += PawnPromotion;
        }
        else if (clickedObject.TryGetComponent(out Bishop bishop)
            && bishop.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = bishop.helperClass;
            bishop.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out King king)
            && king.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = king.helperClass;
            king.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Queen queen)
            && queen.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = queen.helperClass;
            queen.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Knight knight)
            && knight.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = knight.helperClass;
            knight.ShowLegalMoves();
        }
        else if (clickedObject.TryGetComponent(out Rook rook)
            && rook.isWhite == GameModeManager.instance.playerColor)
        {
            selectedPiece = clickedObject;
            pieceHelperScript = rook.helperClass;
            rook.ShowLegalMoves();
        }
        else return false;

        return true;
    }

    private void PawnPromotion(PieceType type, Vector3 position, Piece pawn)
    {
        //Because all the pieces are instantiated with a 0.05f y, the code will fail if 
        //that 0.05f is not added to all pieces.
        CreateNewPiece(type, position + new Vector3(0, 0.05f, 0), pawn);
        DestroyPiece(pawn);
    }

    public bool IsGameOver()
    {
        if (SimulationClass.IsCheckMate(KingHelper.FindWhiteKing()))
        {
            whoWon = true;
            return true;
        }
        else if (SimulationClass.IsCheckMate(KingHelper.FindBlackKing()))
        {
            whoWon = false;
            return true;
        }
        return false;
    }

    IEnumerator CheckMateSteps()
    {
        gameOverText.GetComponent<TMP_Text>().text = "Game Over! \n " + (whoWon ? "White wins!" : "Black wins!");
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator HandlePromotion(char[] mouvementChar, Pawn p)
    {
        promotionSelectionMade = false;
        p.promotionResult += SelectedPromotionPiece;
        p.PromotePawn();

        //waits until a piece has been selected to move
        while (!promotionSelectionMade)
            yield return new WaitForSecondsRealtime(0.05f);

        ExecuteMove();
    }

    private void SelectedPromotionPiece(char selection) 
    {
        if (mouvementChar.Length != 6)
        {
            mouvementChar = (new string(mouvementChar) + selection).ToCharArray();
            promotionSelectionMade = true;
        } else if(mouvementChar.Length == 6)
        {
            mouvementChar[5] = selection;
        }
    }

    private void DestroyPiece(Piece pieceToRemove) => Destroy(pieceToRemove.associatedGameObject);

    private void CreateNewPiece(PieceType type, Vector3 position, Piece piece)
    {
        string color = piece.isWhite ? "White" : "Black";
        string pieceName = color + " " + type.ToString();

        foreach (GameObject model in allPieceModels)
        {
            if (model.name.Equals(pieceName))
            {
                Instantiate(model, position, Quaternion.identity);
                break;
            }
        }
    }

}