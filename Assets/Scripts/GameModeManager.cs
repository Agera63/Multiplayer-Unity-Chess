using UnityEngine;

public class GameModeManager
{
    public static GameModeManager instance = new(); //Singleton

    public GameMode selectedMode { get; private set; }
    public bool playerColor { get; private set; } //True = white | False = black

    public void SetPlayerColor(bool isWhite) => playerColor = isWhite;
    public void SetGameMode(GameMode gameMode) => selectedMode = gameMode;
}
