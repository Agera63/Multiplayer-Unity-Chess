using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MonoBehaviorPiece : MonoBehaviour
{
    [SerializeField] public Collider trigger;
    [SerializeField] public bool isWhite;
    public Piece helperClass;

    private readonly float AnimationTimer = 1f; //seconds
    private float timer = 0;

    /// <summary>
    /// Checks for all legal moves the piece can make.
    /// Afterwards, the legal moves tiles are colored in blue to show the user.
    /// </summary>
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

    /// <summary>
    /// Shows the user a fluent moving animation for a moving piece on the board.
    /// </summary>
    /// <param name="pos">Vector3 of the final position to reach.</param>
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
