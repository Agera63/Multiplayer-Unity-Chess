using JetBrains.Annotations;
using System.Collections;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class Bishop : MonoBehaviorPiece
{
    void Awake()
    {
        helperClass = new BishopHelper(
            isWhite,
            BoardPos.VectorToBoardPosObject(transform.position),
            this.gameObject);
    }
}
