using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardPos
{
    public static HashSet<GameObject> GameTiles = new();

    public int num;
    public int letter;

    public BoardPos(int num, int letter)
    {
        this.num = num;
        this.letter = letter;
    }

    /// <summary>
    /// Allows for BoardPos objects to convert a number to a letter.
    /// IMPORTANT : can only use with BoardPos object.
    /// </summary>
    /// <returns>the letter corresponding to the number on the board.</returns>
    public char NumToLetter()
    {
        char temp = ' ';
        switch (letter)
        {
            case 0: temp = 'a'; break;
            case 1: temp = 'b'; break;
            case 2: temp = 'c'; break;
            case 3: temp = 'd'; break;
            case 4: temp = 'e'; break;
            case 5: temp = 'f'; break;
            case 6: temp = 'g'; break;
            case 7: temp = 'h'; break;
        }
        return temp;
    }

    /// <summary>
    /// Getter for String version of the position
    /// </summary>
    /// <returns>position in string</returns>
    public string PosToString()
    {
        string temp = NumToLetter().ToString() + (num + 1).ToString();
        return temp;
    }

    /// <summary>
    /// Take a String like e4 and turns it into a new BoardPos object
    /// </summary>
    /// <param name="pos">The position we want to convert</param>
    /// <returns>New BoardPos object</returns>
    public static BoardPos StringToPos(string pos)
    {
        int temp = 8;
        // switches the letter to the corresponding number.
        switch (pos[0])
        {
            case 'a': temp = 0; break;
            case 'b': temp = 1; break;
            case 'c': temp = 2; break;
            case 'd': temp = 3; break;
            case 'e': temp = 4; break;
            case 'f': temp = 5; break;
            case 'g': temp = 6; break;
            case 'h': temp = 7; break;
        }
        return new BoardPos(int.Parse(pos[1].ToString()) - 1, temp);
    }

    /// <summary>
    /// Checks the direction of the movement and returns the direction.
    /// </summary>
    /// <param name="start">First position.</param>
    /// <param name="finish">Final position.</param>
    /// <returns>String of movement type.</returns>
    public static string CheckMovementDirection(BoardPos start, BoardPos finish)
    {
        // calculates if its a L movement (knight)
        bool knightMovement =
                (start.letter == finish.letter + 2 && start.num == finish.num - 1) ||
                (start.letter == finish.letter + 2 && start.num == finish.num + 1) ||
                (start.letter == finish.letter + 1 && start.num == finish.num - 2) ||
                (start.letter == finish.letter + 1 && start.num == finish.num + 2) ||
                (start.letter == finish.letter - 2 && start.num == finish.num - 1) ||
                (start.letter == finish.letter - 2 && start.num == finish.num + 1) ||
                (start.letter == finish.letter - 1 && start.num == finish.num - 2) ||
                (start.letter == finish.letter - 1 && start.num == finish.num + 2);
        // calculates if its a diagonal movement
        bool diagonalMovement = Math.Abs(finish.letter - start.letter) == Math.Abs(finish.num - start.num);

        if (start.num != finish.num && start.letter == finish.letter)
        {
            return "vertical";
        }
        else if (start.num == finish.num && start.letter != finish.letter)
        {
            return "horizontal";
        }
        else if (knightMovement)
        {
            return "knight";
        }
        else if (diagonalMovement)
        {
            return "diagonal";
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Checks how many squares the piece is moving
    /// </summary>
    /// <param name="movementType">what type of movement (vertical, horizontal, diagonal)</param>
    /// <param name="start">initial position</param>
    /// <param name="finish">final position</param>
    /// <returns>number of squares it moved</returns>
    public static int SquaresMoved(string movementType, BoardPos start, BoardPos finish)
    {
        int counter = 0;
        if (movementType.Equals("vertical") || movementType.Equals("diagonal"))
        {
            counter = Math.Abs(finish.num - start.num);
        }
        else if (movementType.Equals("horizontal"))
        {
            counter = Math.Abs(finish.letter - start.letter);
        }
        return counter;
    }

    public static Vector3 StringToTileVector3(string position)
    {
        foreach (GameObject go in GameTiles.ToList())
        {
            if (go.TryGetComponent(out BoardTile bt) && bt.boardPosition.Equals(position))
            {
                return go.transform.position;
            }
        }
        return new Vector3(0,0,0);
    }

    public static BoardPos VectorToBoardPosObject(Vector3 position)
    {
        foreach (GameObject go in GameTiles.ToList())
        {
            if (go.TryGetComponent(out BoardTile bt))
            {
                if (bt.transform.position == (position - new Vector3(0, 0.05f, 0))) 
                    return StringToPos(bt.boardPosition);
            }
        }
        return null;
    }

    public static GameObject GetTileByPosition(string pos)
    {
        foreach(GameObject go in GameTiles)
        {
            if (go.TryGetComponent(out BoardTile bt) && bt.boardPosition.Equals(pos))
            {
                return go;
            }
        }
        return null;
    }
}
