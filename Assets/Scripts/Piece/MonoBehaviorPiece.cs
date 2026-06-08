using System.Collections;
using UnityEngine;

public class MonoBehaviorPiece : MonoBehaviour
{
    [SerializeField] public Collider trigger;
    [SerializeField] public bool isWhite;
    public Piece helperClass;

    private readonly float AnimationTimer = 1f; //seconds
    private float timer = 0;

    public void ShowLegalMoves()
    {
        helperClass.legalMoves.Clear();
        helperClass.SetLegalMoves();

        foreach (BoardPos position in helperClass.legalMoves)
        {
            GameObject tile = BoardPos.GetTileByPosition(position.PosToString());
            tile.GetComponent<MeshRenderer>().material.color = Color.blue;
        }
    }

    public IEnumerator MoveAnimation(Vector3 pos)
    {
        //Add sound for every moved piece
        helperClass.HideLegalMoves();
        while (timer <= AnimationTimer)
        {
            timer += Time.deltaTime;

            var percentageMouvementDone = timer / AnimationTimer;

            Vector3 currentPositionByPercentage = Vector3.Lerp(transform.position, pos, percentageMouvementDone);
            transform.position = new Vector3(currentPositionByPercentage.x, currentPositionByPercentage.y, currentPositionByPercentage.z);

            yield return null;
        }

        //Reset everything
        timer = 0;
    }

    private void ActivateTrigger()
    {
        trigger.gameObject.SetActive(true);
    }

    private void DeactivateTrigger()
    {
        trigger.gameObject.SetActive(false);
    }
}
