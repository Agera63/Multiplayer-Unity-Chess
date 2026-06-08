using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class King : MonoBehaviorPiece
{
    void Start()
    {
        helperClass = new KingHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
