using System.Collections;
using TMPro;
using Unity.Netcode;
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

    void Start()
    {
        colorTurn = true;
        selectedPiece = null;

        PieceManager.InitializeBoard();
        PieceManager.removeGameObj += DestroyPiece;

        // Host and offline modes can start immediately
        // Client must wait for color assignment via RPC
        if (GameModeManager.instance.selectedMode != GameMode.PvP_Online
            || (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost))
        {
            AssignControllers();
            StartTurn();
        }
    }

    void Update()
    {
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

    public void InitializeOnlineGame()
    {
        AssignControllers();
        StartTurn();
    }

    private void TryMovePiece(string moveString)
    {
        mouvementChar = moveString.ToCharArray();

        // Send the move to the opponent if we're online and this was our turn
        if (GameModeManager.instance.selectedMode == GameMode.PvP_Online
            && currentController is HumanController)
        {
            NetworkGameManager.Instance.SendMove(moveString);
        }

        Piece movingPiece = Piece.FindPieceAtPos(BoardPos.StringToPos(moveString.Split('-')[0]));

        if (movingPiece is PawnHelper pawnHelper
            && movingPiece.associatedGameObject.TryGetComponent(out Pawn p)
            && p.isGoignToPromoting)
        {
            if (currentController.IsHuman)
            {
                StartCoroutine(HandlePromotion(mouvementChar, p));
                return;
            }
            else
            {
                if (!pawnHelper.CheckPromotionActions(PawnPromotion))
                    pawnHelper.promote += PawnPromotion;
                ExecuteMove();
                return;
            }
        }

        ExecuteMove();
    }

    private void ExecuteMove()
    {
        if (Piece.IsValidMove(mouvementChar, colorTurn))
        {
            PieceManager.Update(mouvementChar);
            colorTurn = !colorTurn;

            selectedPiece = null;
            pieceHelperScript = null;

            if (IsGameOver())
                StartCoroutine(CheckMateSteps());

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
                var netController = new NetworkController();

                // Give NetworkGameManager a reference so RPCs can call ReceiveMove()
                if (NetworkGameManager.Instance != null)
                    NetworkGameManager.Instance.networkController = netController;

                if (playerIsWhite)
                {
                    whiteController = new HumanController();
                    blackController = netController;
                }
                else
                {
                    whiteController = netController;
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
        CreateNewPiece(type, position + new Vector3(0, 0.05f, 0), pawn);
        DestroyPiece(pawn);
    }

    public bool IsGameOver()
    {
        if (SimulationClass.IsCheckMate(KingHelper.FindWhiteKing()))
        {
            whoWon = false;
            return true;
        }
        else if (SimulationClass.IsCheckMate(KingHelper.FindBlackKing()))
        {
            whoWon = true;
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
        if (currentController.IsHuman)
        {
            promotionSelectionMade = false;
            p.promotionResult += SelectedPromotionPiece;
            p.PromotePawn();

            while (!promotionSelectionMade)
                yield return new WaitForSecondsRealtime(0.05f);
        }

        ExecuteMove();
    }

    private void SelectedPromotionPiece(char selection)
    {
        if (mouvementChar.Length != 6)
        {
            mouvementChar = (new string(mouvementChar) + selection).ToCharArray();
            promotionSelectionMade = true;
        }
        else if (mouvementChar.Length == 6)
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