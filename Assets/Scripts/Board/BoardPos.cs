using System;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BoardPos
{
    public int letter;
    public int number;

    public BoardPos(int _letter, int _number)
    {
        letter = _letter;
        number = _number;
    }

    /**
     * Converts chess position to Unity position.
     * Helper methode to get final game object position.
     */
    public Vector3 BoardToUnityPosition(BoardPos _boardPos)
    {
        return new Vector3(0, 0, 0);
    }

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

    public char NumToLetter()
    {
        char temp = ' ';
        switch (number)
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

    public int LetterToNum()
    {
        int temp = 0;
        switch (letter)
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
        return temp;
    }
}
