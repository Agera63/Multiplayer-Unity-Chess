using UnityEngine;

public class PieceManager
{
    public PieceManager instance = new(); //Singleton

    public char[,] Board = new char[8, 8];
}
