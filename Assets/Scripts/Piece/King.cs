using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class King : MonoBehaviorPiece
{
    void Awake()
    {
        helperClass = new KingHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
