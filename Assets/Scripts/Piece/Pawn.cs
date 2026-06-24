using System;
using UnityEngine;

public class Pawn : MonoBehaviorPiece
{
    [SerializeField] private GameObject promotionUi;
    public bool isGoignToPromoting => isWhite ? helperClass.position.num == 6 : helperClass.position.num == 1;
    public event Action<char> promotionResult;

    void Awake()
    {
        helperClass = new PawnHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            gameObject);

        //receives the character to which we need to promote the pawn
        GameMenuController.sendPromotionValue += SendPromotionSelection;
    }

    /// <summary>
    /// Freezes the game and shows the promotionUI so that the player can select to what type of piece 
    /// to promote the pawn to.
    /// </summary>
    public void PromotePawn() 
    {
        promotionUi.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Handles the promotion selection made by the player, resuming the game's time scale,
    /// hiding the promotion UI, and firing the <see cref="promotionResult"/> event
    /// with the selected piece character.
    /// </summary>
    /// <param name="selection">
    /// A char representing the piece the player selected to promote to
    /// ('q' for Queen, 'r' for Rook, 'b' for Bishop, 'n' for Knight).
    /// </param>
    private void SendPromotionSelection(char selection)
    {
        Time.timeScale = 1f;
        promotionUi.gameObject.SetActive(false);
        promotionResult?.Invoke(selection);
    }
}
