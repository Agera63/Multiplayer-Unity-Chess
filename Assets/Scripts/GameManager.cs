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

    private HumanController humanController => currentController as HumanController;

    void Start()
    {
        if (GameModeManager.instance.selectedMode != GameMode.PvP_Online)
            Instantiate(player).GetComponent<CameraScript>()?.PositionCamera();

        colorTurn = true;
        selectedPiece = null;

        PieceManager.InitializeBoard();
        PieceManager.removeGameObj += DestroyPiece;

        bool isOnlineClient = GameModeManager.instance.selectedMode == GameMode.PvP_Online
            && NetworkManager.Singleton != null
            && !NetworkManager.Singleton.IsHost;

        NetworkGameManager.OnOpponentDisconnected += HandleOpponentDisconnected;

        if (!isOnlineClient)
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

    /// <summary>
    /// Starts the turn for the current player by determining whose turn it is
    /// and calling <see cref="IPlayerController.StartTurn"/> on the appropriate controller,
    /// passing <see cref="TryMovePiece"/> as the callback for when a move is ready.
    /// </summary>
    private void StartTurn()
    {
        currentController = colorTurn ? whiteController : blackController;
        currentController.StartTurn((moveString) => TryMovePiece(moveString));
    }

    /// <summary>
    /// Initializes the online game once both players are connected and colors have been assigned.
    /// Called by <see cref="NetworkGameManager"/> on the client side after receiving their color assignment.
    /// </summary>
    public void InitializeOnlineGame()
    {
        AssignControllers();
        StartTurn();
    }

    /// <summary>
    /// Attempts to process a move string received from a controller, handling
    /// pawn promotion separately before delegating to <see cref="ExecuteMove"/>.
    /// Also forwards the move to the opponent over the network if playing online.
    /// </summary>
    /// <param name="moveString">The move to attempt in the format "e2-e4",
    /// with an optional 6th char for pawn promotion (e.g. "e7-e8q").</param>
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

    /// <summary>
    /// Validates and executes the current move, updating the board state, clearing
    /// en passant flags, flipping the turn, and checking for checkmate.
    /// </summary>
    private void ExecuteMove()
    {
        if (Piece.IsValidMove(mouvementChar, colorTurn))
        {
            PieceManager.Update(mouvementChar);

            // En passant is only legal on the turn immediately after the double-step.
            // The mover (colorTurn) just took their move, so the opponent's window
            // to capture en passant has now closed — clear their pawns' flags.
            // The mover's own pawn (if it just double-stepped) is untouched here,
            // since its flag was only just set and is meant to last through the
            // opponent's upcoming turn.
            foreach (Piece p in PieceManager.AllPieces)
            {
                if (p is PawnHelper ph && ph.isWhite != colorTurn)
                    ph.canMove2Squares = false;
            }

            colorTurn = !colorTurn;

            selectedPiece = null;
            pieceHelperScript = null;

            if (IsGameOver())
                StartCoroutine(CheckMateSteps());

            StartTurn();
        }
    }

    /// <summary>
    /// Creates the correct game controllers depending on the player color
    /// and the type of game mode selected in <see cref="GameModeManager"/> class.
    /// </summary>
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

    /// <summary>
    /// Checks if the clicked GameObject returned is a valid MonoBehaviorPiece.
    /// If so, it will also assign that piece to <see cref="selectedPiece"/> 
    /// and the assosiated helperScript to <see cref="pieceHelperScript"/>.
    /// </summary>
    /// <param name="clickedObject">Clicked GameObject returned from <see cref="ClickHelper.FindClickedObject"/></param>
    /// <returns>True if the piece is a valid MonoBehaviorPiece. False otherwise.</returns>
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

    /// <summary>
    /// Calls the creation of a new piece after a pawn has promoted and
    /// destroys the pawn afterwards.
    /// </summary>
    /// <param name="type">A <see cref="PieceType"/> to create on the board.</param>
    /// <param name="position">The position where this new piece should belong.</param>
    /// <param name="pawn">The <see cref="PawnHelper"/> (<see cref="Piece"/>) to be removed.</param>
    private void PawnPromotion(PieceType type, Vector3 position, Piece pawn)
    {
        CreateNewPiece(type, position + new Vector3(0, 0.05f, 0), pawn);
        DestroyPiece(pawn);
    }

    /// <summary>
    /// Every move, it checks if either the white or black king is checkmated.
    /// </summary>
    /// <returns>True if either kings are mated. False otherwise.</returns>
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

    /// <summary>
    /// Shows the winner of the game using <see cref="gameOverText"/> text.
    /// After two seconds of waiting, the user will be sent back to the "MainMenu" scene.
    /// </summary>
    IEnumerator CheckMateSteps()
    {
        gameOverText.GetComponent<TMP_Text>().text = "Game Over! \n " + (whoWon ? "White wins!" : "Black wins!");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Waits for the human player to select a promotion piece via the UI,
    /// then calls <see cref="ExecuteMove"/> once the selection has been made.
    /// </summary>
    /// <param name="mouvementChar">The char array encoding the pawn's move.</param>
    /// <param name="p">The <see cref="Pawn"/> that is being promoted.</param>
    /// <returns>An <see cref="IEnumerator"/> to be used with StartCoroutine.</returns>
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

    /// <summary>
    /// Correctly adds the selected character of the pawn promotion to the <see cref="mouvementChar"/>
    /// </summary>
    /// <param name="selection">The correct character of the piece to be promoted ('q','b','n','r').</param>
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

    /// <summary>
    /// Removes the GameObject of <see cref="Piece"/> that is given.
    /// </summary>
    /// <param name="pieceToRemove">The <see cref="Piece"/> object that need to its GameObject removed.</param>
    private void DestroyPiece(Piece pieceToRemove) => Destroy(pieceToRemove.associatedGameObject);

    /// <summary>
    /// Allows new pieces to be created and positioned correctly on the board.
    /// This is currently only used for pawn promotions.
    /// </summary>
    /// <param name="type">The type of piece that the pawn will promote to.</param>
    /// <param name="position">The vector3 position to place the piece at.</param>
    /// <param name="piece">The newly created piece helper script.</param>
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

    /// <summary>
    /// Handles the opponent disconnecting mid-game by shutting down the network session
    /// and triggering the game over sequence with the local player as the winner.
    /// </summary>
    private void HandleOpponentDisconnected()
    {
        NetworkGameManager.OnOpponentDisconnected -= HandleOpponentDisconnected;
        whoWon = GameModeManager.instance.playerColor;

        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
        }

        StartCoroutine(CheckMateSteps());
    }

    private void OnDestroy()
    {
        NetworkGameManager.OnOpponentDisconnected -= HandleOpponentDisconnected;
        PieceManager.removeGameObj -= DestroyPiece;
    }
}