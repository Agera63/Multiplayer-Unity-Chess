using JetBrains.Annotations;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Bishop : MonoBehaviour
{
    [SerializeField] public Collider trigger;
    [SerializeField] public bool isWhite;
    public BishopHelper helperClass;
    public bool isClicked;

    private readonly float AnimationTimer = 1f; //seconds
    private float timer = 0;

    void Start()
    {
        helperClass = new BishopHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }

    void Update()
    {

    }

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

    public void HideLegalMoves()
    {
        foreach (BoardPos position in helperClass.legalMoves)
        {
            GameObject tile = BoardPos.GetTileByPosition(position.PosToString());
            if (tile.GetComponent<BoardTile>().isWhite)
            {
                tile.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
            else
            {
                tile.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }
    }
    public IEnumerator MoveAnimation(Vector3 pos)
    {
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
        isClicked = false;
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
