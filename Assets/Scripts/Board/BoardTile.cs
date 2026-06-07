using UnityEngine;

public class BoardTile : MonoBehaviour
{
    [SerializeField] public string boardPosition;
    [SerializeField] public bool isWhite;

    void Start()
    {
        //Adds all the board tiles to an hashset
        BoardPos.GameTiles.Add(this.gameObject);
    }
}
