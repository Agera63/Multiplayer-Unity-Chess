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

    public IEnumerator RejectedMoveAnimation()
    {
        float compteur = 1f;
        while (compteur <= 0f)
        {
            transform.position = BoardPos.StringToTileVector3(helperClass.position.PosToString());

            float xPositionOffset = Random.Range(0f, 0.6f);
            float zPositionOffset = Random.Range(0f, 0.6f);

            transform.position = new Vector3(xPositionOffset + transform.position.x, transform.position.y, zPositionOffset + transform.position.z);
            compteur -= Time.deltaTime;

            yield return null;
        }
        transform.position = BoardPos.StringToTileVector3(helperClass.position.PosToString());
    }

    private void ActivateTrigger()
    {
        trigger.gameObject.SetActive(true);
    }

    private void DeactivateTrigger()
    {
        trigger.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        //Checks if the destructon is coming from a scene being loaded
        //or if a capture.
        if (!gameObject.scene.isLoaded)
        {
            PieceManager.AllPieces.Clear();
            PieceManager.SetBoard(new char[8, 8]);
        }
    }
}
