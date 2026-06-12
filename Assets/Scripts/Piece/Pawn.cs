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

    public void PromotePawn() 
    {
        promotionUi.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    private void SendPromotionSelection(char selection)
    {
        Time.timeScale = 1f;
        promotionUi.gameObject.SetActive(false);
        promotionResult?.Invoke(selection);
    }
}
