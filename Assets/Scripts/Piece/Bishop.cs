using JetBrains.Annotations;
using System.Collections;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class Bishop : MonoBehaviorPiece
{
    void Start()
    {
        helperClass = new BishopHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
